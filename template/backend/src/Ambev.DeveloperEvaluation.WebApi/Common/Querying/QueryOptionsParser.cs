using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.WebApi.Common.Querying;


public static class QueryOptionsParser
{
    private const string PageKey = "_page";
    private const string SizeKey = "_size";
    private const string OrderKey = "_order";
    private const string MinPrefix = "_min";
    private const string MaxPrefix = "_max";
    private const char Wildcard = '*';

    public static QueryOptions Parse(IQueryCollection query)
    {
        var (page, size) = ParsePagination(query);
        var filters = ParseFilters(query);
        var order = ParseOrder(query);

        return new QueryOptions(page, size, filters, order);
    }

    private static (int Page, int Size) ParsePagination(IQueryCollection query) =>
        (ParseInt(query, PageKey, QueryOptions.DefaultPage), ParseInt(query, SizeKey, QueryOptions.DefaultSize));

    private static List<Filter> ParseFilters(IQueryCollection query) =>
        query
            .Where(q => q.Key is not (PageKey or SizeKey or OrderKey))
            .SelectMany(q => q.Value.OfType<string>().Select(value => ParseFilter(q.Key, value)))
            .ToList();

    private static Filter ParseFilter(string key, string value)
    {
        if (key.Length > MinPrefix.Length && key.StartsWith(MinPrefix, StringComparison.Ordinal))
            return new Filter(ToCamelCase(key[MinPrefix.Length..]), FilterOperator.GreaterOrEqual, value);

        if (key.Length > MaxPrefix.Length && key.StartsWith(MaxPrefix, StringComparison.Ordinal))
            return new Filter(ToCamelCase(key[MaxPrefix.Length..]), FilterOperator.LessOrEqual, value);

        if (key.Length > 1 && key.EndsWith('!'))
            return new Filter(ToCamelCase(key[..^1]), FilterOperator.NotEqual, value);

        var op = (value.StartsWith(Wildcard), value.EndsWith(Wildcard)) switch
        {
            (true, true) => FilterOperator.Contains,
            (false, true) => FilterOperator.StartsWith,
            (true, false) => FilterOperator.EndsWith,
            _ => FilterOperator.Equal
        };

        return new Filter(ToCamelCase(key), op, value.Trim(Wildcard));
    }

    private static List<Sort> ParseOrder(IQueryCollection query)
    {
        var sorts = new List<Sort>();

        foreach (var value in query[OrderKey].OfType<string>())
        {
            foreach (var part in value.Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var descending = tokens switch
                {
                    [_] => false,
                    [_, var direction] when direction.Equals("asc", StringComparison.OrdinalIgnoreCase) => false,
                    [_, var direction] when direction.Equals("desc", StringComparison.OrdinalIgnoreCase) => true,
                    _ => throw new InvalidQueryException($"Invalid {OrderKey} entry '{part}'. Use 'field', 'field asc' or 'field desc'.")
                };

                sorts.Add(new Sort(ToCamelCase(tokens[0]), descending));
            }
        }

        return sorts;
    }

    private static int ParseInt(IQueryCollection query, string key, int defaultValue)
    {
        var value = query[key].ToString();
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return int.TryParse(value, out var number)
            ? number
            : throw new InvalidQueryException($"Query parameter '{key}' must be an integer.");
    }

    private static string ToCamelCase(string name) =>
        name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
