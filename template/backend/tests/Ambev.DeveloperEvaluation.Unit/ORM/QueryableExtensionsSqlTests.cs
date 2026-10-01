using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Querying;
using Ambev.DeveloperEvaluation.ORM.Querying.FieldMaps;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

public class QueryableExtensionsSqlTests : IDisposable
{
    private static readonly FieldMap<Sale> Fields = SaleFieldMap.Fields;

    private readonly DefaultContext _context = new(new DbContextOptionsBuilder<DefaultContext>()
        .UseNpgsql("Host=localhost;Database=unused")
        .Options);

    public void Dispose() => _context.Dispose();

    private string ToSql(params Filter[] filters) =>
        _context.Sales.ApplyFiltering(filters, Fields).ApplySorting([], Fields, s => s.SaleNumber).ToQueryString();

    [Fact(DisplayName = "Partial string matches should translate to a parameterized ILIKE with escaped wildcards")]
    public void Given_ContainsFilter_When_Translated_Then_ShouldUseILike()
    {
        var sql = ToSql(new Filter("customerName", FilterOperator.Contains, "50%_off"));

        sql.Should().MatchRegex(@"""CustomerName"" ILIKE @\w+ ESCAPE '\\'").And.Contain(@"'%50\%\_off%'");
    }

    [Fact(DisplayName = "Repeated matches on the same field should be combined with OR")]
    public void Given_RepeatedMatchesOnSameField_When_Translated_Then_ShouldUseOr()
    {
        var sql = ToSql(
            new Filter("customerName", FilterOperator.Contains, "UB"),
            new Filter("customerName", FilterOperator.EndsWith, "Zé"),
            new Filter("customerName", FilterOperator.Equal, "Ana"));

        sql.Should().MatchRegex(@"""CustomerName"" ILIKE @\w+ ESCAPE '\\' OR s\.""CustomerName"" ILIKE @\w+ ESCAPE '\\' OR s\.""CustomerName"" = @\w+");
        sql.Should().NotContain(" AND ");
    }

    [Fact(DisplayName = "Filter values should be sent as parameters, not inlined literals")]
    public void Given_RangeFilter_When_Translated_Then_ShouldUseParameters()
    {
        var sql = ToSql(
            new Filter("totalAmount", FilterOperator.GreaterOrEqual, "100.50"),
            new Filter("saleDate", FilterOperator.LessOrEqual, "2026-01-31"));

        sql.Should().MatchRegex(@"""TotalAmount"" >= @\w+").And.MatchRegex(@"""SaleDate"" <= @\w+");
        sql.Should().Contain("='2026-01-31T00:00:00.0000000Z'").And.Contain("='100.50'");
    }
}
