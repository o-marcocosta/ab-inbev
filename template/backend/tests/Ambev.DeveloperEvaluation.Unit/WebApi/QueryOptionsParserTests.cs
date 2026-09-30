using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common.Querying;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.WebApi;

public class QueryOptionsParserTests
{
    private static QueryOptions Parse(string queryString) =>
        QueryOptionsParser.Parse(new QueryCollection(QueryHelpers.ParseQuery(queryString)));

    [Fact(DisplayName = "Empty query should use the default page and size")]
    public void Given_EmptyQuery_When_Parsed_Then_ShouldUseDefaults()
    {
        var options = Parse("");

        options.Page.Should().Be(QueryOptions.DefaultPage);
        options.Size.Should().Be(QueryOptions.DefaultSize);
        options.Filters.Should().BeEmpty();
        options.Sorts.Should().BeEmpty();
    }

    [Fact(DisplayName = "Paging parameters should be read")]
    public void Given_PageAndSize_When_Parsed_Then_ShouldReadThem()
    {
        var options = Parse("?_page=3&_size=25");

        options.Page.Should().Be(3);
        options.Size.Should().Be(25);
    }

    [Theory(DisplayName = "Non-numeric paging parameters should be rejected")]
    [InlineData("?_page=abc")]
    [InlineData("?_size=1.5")]
    public void Given_NonNumericPaging_When_Parsed_Then_ShouldThrow(string query)
    {
        var act = () => Parse(query);

        act.Should().Throw<InvalidQueryException>();
    }

    [Fact(DisplayName = "Order should accept quotes, several keys and a default ascending direction")]
    public void Given_Order_When_Parsed_Then_ShouldReadEachKey()
    {
        var options = Parse("?_order=\"totalAmount desc, SaleDate, customerName ASC\"");

        options.Sorts.Should().Equal(
            new Sort("totalAmount", true),
            new Sort("saleDate", false),
            new Sort("customerName", false));
    }

    [Fact(DisplayName = "Order with an invalid direction should be rejected")]
    public void Given_InvalidOrderDirection_When_Parsed_Then_ShouldThrow()
    {
        var act = () => Parse("?_order=totalAmount sideways");

        act.Should().Throw<InvalidQueryException>().WithMessage("*totalAmount sideways*");
    }

    [Theory(DisplayName = "Filters should map to the operator implied by the key and wildcards")]
    [InlineData("?customerName=Ana", "customerName", FilterOperator.Equal, "Ana")]
    [InlineData("?customerName=*silva*", "customerName", FilterOperator.Contains, "silva")]
    [InlineData("?customerName=Ana*", "customerName", FilterOperator.StartsWith, "Ana")]
    [InlineData("?customerName=*silva", "customerName", FilterOperator.EndsWith, "silva")]
    [InlineData("?_minTotalAmount=50", "totalAmount", FilterOperator.GreaterOrEqual, "50")]
    [InlineData("?_maxSaleDate=2026-01-31", "saleDate", FilterOperator.LessOrEqual, "2026-01-31")]
    [InlineData("?isCancelled!=true", "isCancelled", FilterOperator.NotEqual, "true")]
    public void Given_Filter_When_Parsed_Then_ShouldMapOperator(string query, string field, FilterOperator @operator, string value)
    {
        Parse(query).Filters.Should().ContainSingle().Which.Should().Be(new Filter(field, @operator, value));
    }

    [Fact(DisplayName = "Repeated keys should produce one filter per value")]
    public void Given_RepeatedKey_When_Parsed_Then_ShouldKeepEveryValue()
    {
        var options = Parse("?branchName=North&branchName=South&_page=2");

        options.Filters.Should().Equal(
            new Filter("branchName", FilterOperator.Equal, "North"),
            new Filter("branchName", FilterOperator.Equal, "South"));
    }
}
