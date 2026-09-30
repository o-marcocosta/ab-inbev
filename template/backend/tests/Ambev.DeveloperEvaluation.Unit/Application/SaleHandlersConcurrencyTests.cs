using Ambev.DeveloperEvaluation.Application.Sales.AdjustSaleItemQuantity;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SaleHandlersConcurrencyTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleResultProfile>()).CreateMapper();

    [Fact(DisplayName = "Sale result mapping should be valid")]
    public void SaleResultProfile_ShouldBeValid()
    {
        new MapperConfiguration(cfg => cfg.AddProfile<SaleResultProfile>()).AssertConfigurationIsValid();
    }

    [Fact(DisplayName = "Adjusting quantity should reapply the delta on the newer state after a conflict")]
    public async Task Given_Conflict_When_AdjustingQuantity_Then_ShouldRetryOnReloadedSale()
    {
        // Given: the first load is stale (5 units); meanwhile another request raised it to 7.
        var saleId = Guid.NewGuid();
        var product = SaleTestData.GenerateProduct();
        var stale = BuildSale(product, 5);
        var current = BuildSale(product, 7);
        var itemId = stale.Items.Single().Id;
        SetItemId(current, itemId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(stale, current);
        _saleRepository.UpdateAsync(stale, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConcurrencyConflictException("changed"));

        var handler = new AdjustSaleItemQuantityHandler(_saleRepository, _mapper);

        // When
        var result = await handler.Handle(
            new AdjustSaleItemQuantityCommand { SaleId = saleId, ItemId = itemId, Delta = 1 }, CancellationToken.None);

        // Then: both changes are kept (7 + 1), not overwritten (5 + 1).
        result.Items.Single().Quantity.Should().Be(8);
        await _saleRepository.Received(1).UpdateAsync(current, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Adjusting quantity should give up after the maximum number of attempts")]
    public async Task Given_PersistentConflict_When_AdjustingQuantity_Then_ShouldThrowAfterMaxAttempts()
    {
        var product = SaleTestData.GenerateProduct();
        var sale = BuildSale(product, 5);
        var itemId = sale.Items.Single().Id;

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(_ => BuildSaleWithItemId(product, 5, itemId));
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConcurrencyConflictException("changed"));

        var handler = new AdjustSaleItemQuantityHandler(_saleRepository, _mapper);

        var act = () => handler.Handle(
            new AdjustSaleItemQuantityCommand { SaleId = sale.Id, ItemId = itemId, Delta = 1 }, CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
        await _saleRepository.Received(3).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Adjusting an unknown item should return not found")]
    public async Task Given_UnknownItem_When_AdjustingQuantity_Then_ShouldThrowNotFound()
    {
        var sale = BuildSale(SaleTestData.GenerateProduct(), 5);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var handler = new AdjustSaleItemQuantityHandler(_saleRepository, _mapper);

        var act = () => handler.Handle(
            new AdjustSaleItemQuantityCommand { SaleId = sale.Id, ItemId = Guid.NewGuid(), Delta = 1 }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Updating sale details should not retry on conflict")]
    public async Task Given_Conflict_When_UpdatingDetails_Then_ShouldSurfaceConflict()
    {
        var sale = BuildSale(SaleTestData.GenerateProduct(), 5);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConcurrencyConflictException("changed"));

        var customer = SaleTestData.GenerateCustomer();
        var branch = SaleTestData.GenerateBranch();
        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleDate = DateTime.UtcNow,
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            BranchId = branch.Id,
            BranchName = branch.Name
        };

        var act = () => new UpdateSaleHandler(_saleRepository, _mapper).Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    private static Sale BuildSale(ProductRef product, int quantity)
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(product, quantity, 10m);
        return sale;
    }

    private static Sale BuildSaleWithItemId(ProductRef product, int quantity, Guid itemId)
    {
        var sale = BuildSale(product, quantity);
        SetItemId(sale, itemId);
        return sale;
    }

    // Two loads of the same sale share item ids; in memory each BuildSale generates new ones.
    private static void SetItemId(Sale sale, Guid itemId) => sale.Items.Single().Id = itemId;
}
