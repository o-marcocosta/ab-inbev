using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Querying;

public static class QueryableExtensions
{
    private static readonly HashSet<Type> ComparableTypes =
        [typeof(int), typeof(long), typeof(decimal), typeof(double), typeof(DateTime), typeof(DateOnly)];

    private static readonly HashSet<FilterOperator> MatchOperators =
        [FilterOperator.Equal, FilterOperator.Contains, FilterOperator.StartsWith, FilterOperator.EndsWith];

    // Without an explicit escape character Npgsql emits ESCAPE '', which would turn escaped % and _ back into wildcards.
    private const string LikeEscape = @"\";

    private static readonly MethodInfo ILikeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
        nameof(NpgsqlDbFunctionsExtensions.ILike), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])!;

    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, IEnumerable<Filter> filters, FieldMap<T> fields)
    {
        foreach (var group in filters.GroupBy(f => f.Field, StringComparer.OrdinalIgnoreCase))
        {
            var selector = fields.Get(group.Key);

            var matches = group.Where(f => MatchOperators.Contains(f.Operator))
                .Select(f => BuildPredicate(selector, f))
                .ToList();
            if (matches.Count > 0)
                query = query.Where(ToLambda<T>(matches.Aggregate(Expression.OrElse), selector));

            foreach (var filter in group.Where(f => !MatchOperators.Contains(f.Operator)))
                query = query.Where(ToLambda<T>(BuildPredicate(selector, filter), selector));
        }

        return query;
    }

    // The default sort is always appended as the last key, so pages stay stable when the requested keys tie.
    public static IQueryable<T> ApplySorting<T, TKey>(this IQueryable<T> query, IEnumerable<Sort> sorts,
        FieldMap<T> fields, Expression<Func<T, TKey>> defaultSort)
    {
        IOrderedQueryable<T>? ordered = null;

        foreach (var sort in sorts)
        {
            var selector = fields.Get(sort.Field);
            var method = (ordered is null, sort.Descending) switch
            {
                (true, false) => nameof(Queryable.OrderBy),
                (true, true) => nameof(Queryable.OrderByDescending),
                (false, false) => nameof(Queryable.ThenBy),
                (false, true) => nameof(Queryable.ThenByDescending)
            };

            var source = ordered ?? query;
            var call = Expression.Call(typeof(Queryable), method, [typeof(T), selector.ReturnType],
                source.Expression, Expression.Quote(selector));
            ordered = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }

        return ordered is null ? query.OrderBy(defaultSort) : ordered.ThenBy(defaultSort);
    }

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int size) =>
        query.Skip((page - 1) * size).Take(size);

    private static Expression BuildPredicate(LambdaExpression selector, Filter filter)
    {
        var field = selector.Body;
        var type = Nullable.GetUnderlyingType(field.Type) ?? field.Type;

        return filter.Operator switch
        {
            FilterOperator.Equal => Expression.Equal(field, AsParameter(Convert(filter, type), field.Type)),
            FilterOperator.NotEqual => Expression.NotEqual(field, AsParameter(Convert(filter, type), field.Type)),
            FilterOperator.Contains => BuildLikePredicate(field, type, filter, value => $"%{value}%"),
            FilterOperator.StartsWith => BuildLikePredicate(field, type, filter, value => $"{value}%"),
            FilterOperator.EndsWith => BuildLikePredicate(field, type, filter, value => $"%{value}"),
            FilterOperator.GreaterOrEqual => BuildComparisonPredicate(field, type, filter, Expression.GreaterThanOrEqual),
            FilterOperator.LessOrEqual => BuildComparisonPredicate(field, type, filter, Expression.LessThanOrEqual),
            _ => throw Unsupported(filter)
        };
    }

    private static MethodCallExpression BuildLikePredicate(Expression field, Type type, Filter filter,
    Func<string, string> toPattern)
    {
        if (type != typeof(string)) throw Unsupported(filter);

        var pattern = toPattern(EscapeLikePattern(filter.Value));
        return Expression.Call(ILikeMethod, Expression.Constant(EF.Functions), field,
            AsParameter(pattern, typeof(string)), Expression.Constant(LikeEscape));
    }

    private static BinaryExpression BuildComparisonPredicate(Expression field, Type type, Filter filter,
        Func<Expression, Expression, BinaryExpression> compare)
    {
        if (!ComparableTypes.Contains(type)) throw Unsupported(filter);

        return compare(field, AsParameter(Convert(filter, type), field.Type));
    }

    private static object Convert(Filter filter, Type type)
    {
        try
        {
            if (type == typeof(string)) return filter.Value;
            if (type == typeof(Guid)) return Guid.Parse(filter.Value);
            if (type.IsEnum) return Enum.Parse(type, filter.Value, ignoreCase: true);
            // Dates are stored as UTC (timestamptz): values without an offset are taken as UTC.
            if (type == typeof(DateTime))
                return DateTime.Parse(filter.Value, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            if (type == typeof(DateOnly)) return DateOnly.Parse(filter.Value, CultureInfo.InvariantCulture);

            return System.Convert.ChangeType(filter.Value, type, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException or InvalidCastException)
        {
            throw new InvalidQueryException($"Value '{filter.Value}' is not valid for field '{filter.Field}'.");
        }
    }

    // Wrapping the value in an object makes EF Core send it as a SQL parameter instead of inlining a literal.
    private static MemberExpression AsParameter(object value, Type type)
    {
        var holder = Activator.CreateInstance(typeof(ValueHolder<>).MakeGenericType(type), value)!;
        return Expression.Property(Expression.Constant(holder), nameof(ValueHolder<object>.Value));
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace(LikeEscape, LikeEscape + LikeEscape).Replace("%", LikeEscape + "%").Replace("_", LikeEscape + "_");

    private static Expression<Func<T, bool>> ToLambda<T>(Expression body, LambdaExpression selector) =>
        Expression.Lambda<Func<T, bool>>(body, selector.Parameters);

    private static InvalidQueryException Unsupported(Filter filter) =>
        new($"Operator '{filter.Operator}' is not supported for field '{filter.Field}'.");

    private sealed class ValueHolder<TValue>(TValue value)
    {
        public TValue Value { get; } = value;
    }
}
