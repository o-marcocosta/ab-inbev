namespace Ambev.DeveloperEvaluation.Domain.Common.Querying;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int Size)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Size);

    public PagedResult<TOut> Map<TOut>(Func<IReadOnlyList<T>, IReadOnlyList<TOut>> map) =>
        new(map(Items), TotalCount, Page, Size);
}
