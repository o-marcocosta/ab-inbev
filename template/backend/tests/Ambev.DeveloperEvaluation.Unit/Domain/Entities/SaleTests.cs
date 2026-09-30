using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Created sale should have id, UTC date and zero total")]
    public void Given_ValidData_When_Created_Then_ShouldInitializeSale()
    {
        var sale = SaleTestData.GenerateValidSale();

        sale.Id.Should().NotBeEmpty();
        sale.SaleDate.Kind.Should().Be(DateTimeKind.Utc);
        sale.TotalAmount.Should().Be(0);
        sale.IsCancelled.Should().BeFalse();
        sale.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Adding an item should persist the discount snapshot and update the total")]
    public void Given_Item_When_Added_Then_ShouldApplyDiscountAndTotal()
    {
        var sale = SaleTestData.GenerateValidSale();

        var item = sale.AddItem(SaleTestData.GenerateProduct(), 10, 12.50m);

        item.SaleId.Should().Be(sale.Id);
        item.DiscountRate.Should().Be(0.20m);
        item.DiscountAmount.Should().Be(25.00m);
        item.TotalAmount.Should().Be(100.00m);
        sale.TotalAmount.Should().Be(100.00m);
    }

    [Fact(DisplayName = "Discount amount should be rounded half away from zero")]
    public void Given_FractionalDiscount_When_Added_Then_ShouldRoundToTwoDecimals()
    {
        var sale = SaleTestData.GenerateValidSale();

        // gross = 4 * 3.33 = 13.32 -> 10% = 1.332 -> 1.33
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 4, 3.33m);

        item.DiscountAmount.Should().Be(1.33m);
        item.TotalAmount.Should().Be(11.99m);
    }

    [Fact(DisplayName = "Same product added twice should be consolidated into one line")]
    public void Given_SameProductTwice_When_Added_Then_ShouldConsolidateAndRecalculateTier()
    {
        var sale = SaleTestData.GenerateValidSale();
        var product = SaleTestData.GenerateProduct();

        sale.AddItem(product, 3, 10m);
        sale.AddItem(product, 7, 10m);

        var item = sale.Items.Should().ContainSingle().Subject;
        item.Quantity.Should().Be(10);
        item.DiscountRate.Should().Be(0.20m);
        sale.TotalAmount.Should().Be(80m);
    }

    [Fact(DisplayName = "Consolidation exceeding 20 identical items should be rejected")]
    public void Given_ConsolidatedQuantityAbove20_When_Added_Then_ShouldThrow()
    {
        var sale = SaleTestData.GenerateValidSale();
        var product = SaleTestData.GenerateProduct();
        sale.AddItem(product, 15, 10m);

        var act = () => sale.AddItem(product, 6, 10m);

        act.Should().Throw<DomainException>();
        sale.Items.Single().Quantity.Should().Be(15);
    }

    [Fact(DisplayName = "Same product with different unit prices should be rejected")]
    public void Given_SameProductWithDifferentPrice_When_Added_Then_ShouldThrow()
    {
        var sale = SaleTestData.GenerateValidSale();
        var product = SaleTestData.GenerateProduct();
        sale.AddItem(product, 2, 10m);

        var act = () => sale.AddItem(product, 2, 11m);

        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "Invalid item data should be rejected")]
    [InlineData(0, 10)]
    [InlineData(21, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 10.555)]
    public void Given_InvalidItemData_When_Added_Then_ShouldThrow(int quantity, double unitPrice)
    {
        var sale = SaleTestData.GenerateValidSale();

        var act = () => sale.AddItem(SaleTestData.GenerateProduct(), quantity, (decimal)unitPrice);

        act.Should().Throw<DomainException>();
        sale.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Adjusting quantity down should move the item to the new discount tier")]
    public void Given_Item_When_QuantityReduced_Then_ShouldRecalculateTier()
    {
        var sale = SaleTestData.GenerateValidSale();
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 10, 10m);

        sale.AdjustItemQuantity(item.Id, -1);

        item.Quantity.Should().Be(9);
        item.DiscountRate.Should().Be(0.10m);
        item.TotalAmount.Should().Be(81m);
        sale.TotalAmount.Should().Be(81m);
    }

    [Fact(DisplayName = "Adjusting quantity up should add to the current quantity")]
    public void Given_Item_When_QuantityIncreased_Then_ShouldAddDelta()
    {
        var sale = SaleTestData.GenerateValidSale();
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 3, 10m);

        sale.AdjustItemQuantity(item.Id, 2);

        item.Quantity.Should().Be(5);
        item.DiscountRate.Should().Be(0.10m);
        sale.TotalAmount.Should().Be(45m);
    }

    [Theory(DisplayName = "Adjustments that are zero or leave the quantity outside 1-20 should be rejected")]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(-6)]
    [InlineData(16)]
    public void Given_Item_When_InvalidAdjustment_Then_ShouldThrow(int delta)
    {
        var sale = SaleTestData.GenerateValidSale();
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 5, 10m);

        var act = () => sale.AdjustItemQuantity(item.Id, delta);

        act.Should().Throw<DomainException>();
        item.Quantity.Should().Be(5);
        sale.TotalAmount.Should().Be(item.TotalAmount);
    }

    [Fact(DisplayName = "Adjusting a cancelled item should be rejected")]
    public void Given_CancelledItem_When_Adjusted_Then_ShouldThrow()
    {
        var sale = SaleTestData.GenerateValidSale();
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 5, 10m);
        sale.CancelItem(item.Id);

        var act = () => sale.AdjustItemQuantity(item.Id, 1);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Cancelled items should be kept but excluded from the total")]
    public void Given_Items_When_OneCancelled_Then_TotalShouldIgnoreIt()
    {
        var sale = SaleTestData.GenerateValidSale();
        var kept = sale.AddItem(SaleTestData.GenerateProduct(), 2, 10m);
        var cancelled = sale.AddItem(SaleTestData.GenerateProduct(), 5, 10m);

        sale.CancelItem(cancelled.Id);

        sale.Items.Should().HaveCount(2);
        cancelled.IsCancelled.Should().BeTrue();
        cancelled.CancelledAt.Should().NotBeNull();
        sale.TotalAmount.Should().Be(kept.TotalAmount);
    }

    [Fact(DisplayName = "Gross and discount amounts should add up to the total and ignore cancelled items")]
    public void Given_Items_When_OneCancelled_Then_GrossAndDiscountShouldIgnoreIt()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(SaleTestData.GenerateProduct(), 10, 12.50m); // gross 125.00, discount 25.00
        sale.AddItem(SaleTestData.GenerateProduct(), 4, 3.33m);   // gross 13.32, discount 1.33
        var cancelled = sale.AddItem(SaleTestData.GenerateProduct(), 5, 10m);

        sale.CancelItem(cancelled.Id);

        sale.GrossAmount.Should().Be(138.32m);
        sale.DiscountAmount.Should().Be(26.33m);
        sale.TotalAmount.Should().Be(sale.GrossAmount - sale.DiscountAmount);
    }

    [Fact(DisplayName = "Re-adding a product after cancelling its line should create a new line")]
    public void Given_CancelledLine_When_SameProductAdded_Then_ShouldCreateNewLine()
    {
        var sale = SaleTestData.GenerateValidSale();
        var product = SaleTestData.GenerateProduct();
        var first = sale.AddItem(product, 5, 10m);
        sale.CancelItem(first.Id);

        var second = sale.AddItem(product, 2, 10m);

        second.Id.Should().NotBe(first.Id);
        sale.TotalAmount.Should().Be(20m);
    }

    [Fact(DisplayName = "Cancelling an already cancelled item should be rejected")]
    public void Given_CancelledItem_When_CancelledAgain_Then_ShouldThrow()
    {
        var sale = SaleTestData.GenerateValidSale();
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 1, 10m);
        sale.CancelItem(item.Id);

        var act = () => sale.CancelItem(item.Id);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Unknown item id should be rejected")]
    public void Given_UnknownItemId_When_Cancelled_Then_ShouldThrow()
    {
        var sale = SaleTestData.GenerateValidSale();

        var act = () => sale.CancelItem(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "A cancelled sale should not accept changes")]
    public void Given_CancelledSale_When_Modified_Then_ShouldThrow()
    {
        var sale = SaleTestData.GenerateValidSale();
        var item = sale.AddItem(SaleTestData.GenerateProduct(), 1, 10m);

        sale.Cancel();

        sale.IsCancelled.Should().BeTrue();
        sale.CancelledAt.Should().NotBeNull();
        FluentActions.Invoking(() => sale.AddItem(SaleTestData.GenerateProduct(), 1, 10m)).Should().Throw<DomainException>();
        FluentActions.Invoking(() => sale.AdjustItemQuantity(item.Id, 1)).Should().Throw<DomainException>();
        FluentActions.Invoking(() => sale.CancelItem(item.Id)).Should().Throw<DomainException>();
        FluentActions.Invoking(() => sale.UpdateDetails(DateTime.UtcNow, SaleTestData.GenerateCustomer(), SaleTestData.GenerateBranch()))
            .Should().Throw<DomainException>();
        FluentActions.Invoking(sale.Cancel).Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Updating the details should replace date, customer and branch")]
    public void Given_Sale_When_DetailsUpdated_Then_ShouldReplaceValues()
    {
        var sale = SaleTestData.GenerateValidSale();
        var customer = SaleTestData.GenerateCustomer();
        var branch = SaleTestData.GenerateBranch();
        var date = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);

        sale.UpdateDetails(date, customer, branch);

        sale.Customer.Should().Be(customer);
        sale.Branch.Should().Be(branch);
        sale.SaleDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact(DisplayName = "External references should reject empty id or name")]
    public void Given_InvalidExternalReference_When_Created_Then_ShouldThrow()
    {
        FluentActions.Invoking(() => new CustomerRef(Guid.Empty, "Name")).Should().Throw<DomainException>();
        FluentActions.Invoking(() => new BranchRef(Guid.NewGuid(), " ")).Should().Throw<DomainException>();
    }
}
