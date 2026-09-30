using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Policies;

public class QuantityDiscountPolicyTests
{
    [Theory(DisplayName = "Discount rate should follow the quantity tiers")]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void Given_ValidQuantity_When_GettingRate_Then_ShouldReturnTierRate(int quantity, double expectedRate)
    {
        var rate = QuantityDiscountPolicy.GetDiscountRate(quantity);

        rate.Should().Be((decimal)expectedRate);
    }

    [Theory(DisplayName = "Quantities outside 1-20 should be rejected")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void Given_InvalidQuantity_When_GettingRate_Then_ShouldThrow(int quantity)
    {
        var act = () => QuantityDiscountPolicy.GetDiscountRate(quantity);

        act.Should().Throw<DomainException>();
    }
}
