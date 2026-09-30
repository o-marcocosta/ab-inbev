using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.ORM.Querying;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

// Runs the generated expression trees with LINQ to Objects. ILIKE (partial string matches) only
// translates in PostgreSQL, so those operators are covered here just for their type checks.
public class QueryableExtensionsTests
{
    private sealed record Row(int Number, string Name, decimal Amount, DateTime Date, DateTime? ClosedAt, bool Active, Guid RefId);

    private static readonly Guid RefA = Guid.NewGuid();

    private static readonly Row[] Rows =
    [
        new(1, "alpha", 10m, new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, true, RefA),
        new(2, "beta", 50m, new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc), false, Guid.NewGuid()),
        new(3, "gamma", 50m, new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc), null, true, Guid.NewGuid()),
        new(4, "delta", 200m, new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), null, false, RefA)
    ];

    private static readonly FieldMap<Row> Fields = new FieldMap<Row>()
        .Add("number", r => r.Number)
        .Add("name", r => r.Name)
        .Add("amount", r => r.Amount)
        .Add("date", r => r.Date)
        .Add("closedAt", r => r.ClosedAt)
        .Add("active", r => r.Active)
        .Add("refId", r => r.RefId);

    private static int[] FilterRows(params Filter[] filters) =>
        Rows.AsQueryable().ApplyFiltering(filters, Fields).Select(r => r.Number).ToArray();

    [Fact(DisplayName = "Equality filters on different fields should be combined with AND")]
    public void Given_EqualitiesOnDifferentFields_When_Filtering_Then_ShouldMatchAll()
    {
        FilterRows(new Filter("amount", FilterOperator.Equal, "50"), new Filter("active", FilterOperator.Equal, "true"))
            .Should().Equal(3);
    }

    [Fact(DisplayName = "Repeated equality filters on the same field should be combined with OR")]
    public void Given_RepeatedEqualityOnSameField_When_Filtering_Then_ShouldMatchAny()
    {
        FilterRows(new Filter("number", FilterOperator.Equal, "1"), new Filter("number", FilterOperator.Equal, "4"))
            .Should().Equal(1, 4);
    }

    [Fact(DisplayName = "Range filters should be inclusive and dates taken as UTC")]
    public void Given_DateRange_When_Filtering_Then_ShouldIncludeBounds()
    {
        FilterRows(new Filter("date", FilterOperator.GreaterOrEqual, "2026-02-10"), new Filter("date", FilterOperator.LessOrEqual, "2026-03-10"))
            .Should().Equal(2, 3);
    }

    [Fact(DisplayName = "Range filters should work on nullable fields")]
    public void Given_NullableField_When_FilteringByRange_Then_ShouldSkipNulls()
    {
        FilterRows(new Filter("closedAt", FilterOperator.GreaterOrEqual, "2026-01-01")).Should().Equal(2);
    }

    [Fact(DisplayName = "Not-equal filter should exclude the value")]
    public void Given_NotEqual_When_Filtering_Then_ShouldExcludeValue()
    {
        FilterRows(new Filter("refId", FilterOperator.NotEqual, RefA.ToString())).Should().Equal(2, 3);
    }

    [Fact(DisplayName = "Repeated not-equal filters on the same field should be combined with AND")]
    public void Given_RepeatedNotEqualOnSameField_When_Filtering_Then_ShouldExcludeAll()
    {
        FilterRows(new Filter("number", FilterOperator.NotEqual, "1"), new Filter("number", FilterOperator.NotEqual, "4"))
            .Should().Equal(2, 3);
    }

    [Fact(DisplayName = "A range and an equality on the same field should both apply")]
    public void Given_RangeAndEqualityOnSameField_When_Filtering_Then_ShouldMatchBoth()
    {
        FilterRows(new Filter("amount", FilterOperator.Equal, "50"), new Filter("amount", FilterOperator.GreaterOrEqual, "100"))
            .Should().BeEmpty();
    }

    [Fact(DisplayName = "Field names should be matched case-insensitively")]
    public void Given_DifferentCasing_When_Filtering_Then_ShouldResolveField()
    {
        FilterRows(new Filter("Amount", FilterOperator.Equal, "200")).Should().Equal(4);
    }

    [Fact(DisplayName = "Unknown fields should be rejected")]
    public void Given_UnknownField_When_Filtering_Then_ShouldThrow()
    {
        var act = () => FilterRows(new Filter("secret", FilterOperator.Equal, "1"));

        act.Should().Throw<InvalidQueryException>().WithMessage("*secret*");
    }

    [Theory(DisplayName = "Values that do not fit the field type should be rejected")]
    [InlineData("amount", "abc")]
    [InlineData("date", "not-a-date")]
    [InlineData("refId", "123")]
    [InlineData("active", "maybe")]
    public void Given_InvalidValue_When_Filtering_Then_ShouldThrow(string field, string value)
    {
        var act = () => FilterRows(new Filter(field, FilterOperator.Equal, value));

        act.Should().Throw<InvalidQueryException>().WithMessage($"*{field}*");
    }

    [Theory(DisplayName = "Operators that do not apply to the field type should be rejected")]
    [InlineData("amount", FilterOperator.Contains)]
    [InlineData("name", FilterOperator.GreaterOrEqual)]
    [InlineData("refId", FilterOperator.LessOrEqual)]
    public void Given_UnsupportedOperator_When_Filtering_Then_ShouldThrow(string field, FilterOperator @operator)
    {
        var act = () => FilterRows(new Filter(field, @operator, "1"));

        act.Should().Throw<InvalidQueryException>();
    }

    [Fact(DisplayName = "Sorting should apply keys in order and break ties with the default sort")]
    public void Given_MultipleSorts_When_Sorting_Then_ShouldOrderByEachKey()
    {
        var result = Rows.AsQueryable()
            .ApplySorting([new Sort("amount", Descending: true)], Fields, r => r.Number)
            .Select(r => r.Number);

        result.Should().Equal(4, 2, 3, 1);
    }

    [Fact(DisplayName = "Without sorts the default sort should be used")]
    public void Given_NoSorts_When_Sorting_Then_ShouldUseDefault()
    {
        var result = Rows.Reverse().AsQueryable()
            .ApplySorting([], Fields, r => r.Number)
            .Select(r => r.Number);

        result.Should().Equal(1, 2, 3, 4);
    }

    [Fact(DisplayName = "Sorting by an unknown field should be rejected")]
    public void Given_UnknownSortField_When_Sorting_Then_ShouldThrow()
    {
        var act = () => Rows.AsQueryable().ApplySorting([new Sort("secret", false)], Fields, r => r.Number).ToList();

        act.Should().Throw<InvalidQueryException>();
    }

    [Fact(DisplayName = "Pagination should skip the previous pages")]
    public void Given_PageAndSize_When_Paginating_Then_ShouldReturnThatPage()
    {
        Rows.AsQueryable().ApplyPagination(page: 2, size: 3).Select(r => r.Number).Should().Equal(4);
    }
}
