using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.ORM.Querying;

// Whitelist of the fields a client may filter and sort by, keyed by their name in the API (JSON) contract.
// Anything not listed here is rejected, so internal columns never become queryable by accident.
public sealed class FieldMap<T>
{
    private readonly Dictionary<string, LambdaExpression> _fields = new(StringComparer.OrdinalIgnoreCase);

    public FieldMap<T> Add<TField>(string name, Expression<Func<T, TField>> selector)
    {
        _fields.Add(name, selector);
        return this;
    }

    public LambdaExpression Get(string name) =>
        _fields.TryGetValue(name, out var selector)
            ? selector
            : throw new InvalidQueryException(
                $"Field '{name}' cannot be used to filter or sort. Allowed fields: {string.Join(", ", _fields.Keys)}.");
}
