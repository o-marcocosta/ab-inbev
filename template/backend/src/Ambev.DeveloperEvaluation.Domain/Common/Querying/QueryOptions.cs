namespace Ambev.DeveloperEvaluation.Domain.Common.Querying;

public sealed record QueryOptions(int Page, int Size, IReadOnlyList<Filter> Filters, IReadOnlyList<Sort> Sorts)
{
    public const int DefaultPage = 1;
    public const int DefaultSize = 10;
    public const int MaxSize = 100;
}
