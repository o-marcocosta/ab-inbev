using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Sales.AddSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SaleHandlersOutboxTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IOutbox _outbox = Substitute.For<IOutbox>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleResultProfile>()).CreateMapper();

    [Fact(DisplayName = "Creating a sale should announce SaleCreated once, after saving")]
    public async Task Given_ValidCommand_When_Created_Then_ShouldAddSaleCreated()
    {
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());
        var product = SaleTestData.GenerateProduct();
        var command = new CreateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new CreateSaleCommand.Item { ProductId = product.Id, ProductName = product.Name, Quantity = 3, UnitPrice = 10m },
                new CreateSaleCommand.Item { ProductId = product.Id, ProductName = product.Name, Quantity = 2, UnitPrice = 10m }
            ]
        };

        var result = await new CreateSaleHandler(_saleRepository, _outbox, _mapper).Handle(command, CancellationToken.None);

        _outbox.Received(1).Add(Arg.Is<SaleCreatedIntegrationEvent>(e => e.SaleId == result.Id && e.Items.Single().Quantity == 5));
        _outbox.ReceivedWithAnyArgs(1).Add(default!);
    }

    [Fact(DisplayName = "Adding an item should announce SaleModified")]
    public async Task Given_Sale_When_ItemAdded_Then_ShouldAddSaleModified()
    {
        var sale = LoadSale();
        var product = SaleTestData.GenerateProduct();

        await new AddSaleItemHandler(_saleRepository, _outbox, _mapper).Handle(
            new AddSaleItemCommand { SaleId = sale.Id, ProductId = product.Id, ProductName = product.Name, Quantity = 1, UnitPrice = 10m },
            CancellationToken.None);

        _outbox.Received(1).Add(Arg.Is<SaleModifiedIntegrationEvent>(e => e.SaleId == sale.Id && e.Items.Count == 2));
    }

    [Fact(DisplayName = "Cancelling a sale should announce SaleCancelled")]
    public async Task Given_Sale_When_Cancelled_Then_ShouldAddSaleCancelled()
    {
        var sale = LoadSale();

        await new CancelSaleHandler(_saleRepository, _outbox, _mapper).Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        _outbox.Received(1).Add(Arg.Is<SaleCancelledIntegrationEvent>(e => e.SaleId == sale.Id));
    }

    [Fact(DisplayName = "Cancelling an item should announce ItemCancelled for that item")]
    public async Task Given_Sale_When_ItemCancelled_Then_ShouldAddItemCancelled()
    {
        var sale = LoadSale();
        var item = sale.Items.Single();

        await new CancelSaleItemHandler(_saleRepository, _outbox, _mapper).Handle(
            new CancelSaleItemCommand(sale.Id, item.Id), CancellationToken.None);

        _outbox.Received(1).Add(Arg.Is<SaleItemCancelledIntegrationEvent>(e => e.ItemId == item.Id && e.SaleTotalAmount == 0));
    }

    [Fact(DisplayName = "A change that was not saved should not be announced")]
    public async Task Given_SaveFails_When_Cancelled_Then_ShouldNotAddMessage()
    {
        var sale = LoadSale();
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("db down"));

        var act = () => new CancelSaleHandler(_saleRepository, _outbox, _mapper).Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _outbox.DidNotReceiveWithAnyArgs().Add(default!);
    }

    private Sale LoadSale()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(SaleTestData.GenerateProduct(), 5, 10m);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        return sale;
    }
}
