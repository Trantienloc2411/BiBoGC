using FluentAssertions;
using Moq;
using Sale.Application.Commands.GenerateInvoice;
using Sale.Application.Interfaces;
using Sale.Domain.Domain;
using Sale.Domain.Enum;
using Sale.Domain.ValueObjects;
using Shared.Application.Interfaces;

namespace BiBoGC.Tests.Unit.Application;

public class GenerateInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock = new();
    private readonly Mock<IInvoiceNumberGenerator> _numGenMock = new();
    private readonly Mock<ISalesOrderRepository> _orderRepoMock = new();
    private readonly Mock<IStoreInfoService> _storeInfoMock = new();
    private readonly Mock<ISaleUnitOfWork> _uowMock = new();

    private GenerateInvoiceCommandHandler CreateHandler()
    {
        return new GenerateInvoiceCommandHandler(_orderRepoMock.Object, _invoiceRepoMock.Object,
            _numGenMock.Object, _storeInfoMock.Object, _uowMock.Object);
    }

    private static StoreInfo DefaultStore()
    {
        return new StoreInfo("BiBo Store", "123 Lê Lợi, HCM", "028-1234-5678", "0123456789");
    }

    private static SalesOrder CompletedOrder()
    {
        var order = new SalesOrder("ORD-001", PaymentMethod.Cash);
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), null,
            "Sản phẩm A", "Loại 1", "SKU-A", "chai", 2, 50_000m);
        order.ApplyTax(0m);
        order.Complete(100_000m);
        return order;
    }

    private void SetupTransaction()
    {
        _uowMock.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), default))
            .Returns((Func<Task> action, CancellationToken _) => action());
    }

    // ── Order not found ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _invoiceRepoMock.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Invoice?)null);
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((SalesOrder?)null);

        var result = await CreateHandler().Handle(new GenerateInvoiceCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Không tìm thấy"));
    }

    // ── Order not completed ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DraftOrder_ReturnsFailure()
    {
        var order = new SalesOrder("ORD-DRAFT", PaymentMethod.Cash);
        _invoiceRepoMock.Setup(r => r.GetByOrderIdAsync(order.Id, default)).ReturnsAsync((Invoice?)null);
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(order.Id, default)).ReturnsAsync(order);

        var result = await CreateHandler().Handle(new GenerateInvoiceCommand(order.Id), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("hoàn thành"));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CompletedOrder_GeneratesInvoice()
    {
        var order = CompletedOrder();
        _invoiceRepoMock.Setup(r => r.GetByOrderIdAsync(order.Id, default)).ReturnsAsync((Invoice?)null);
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(order.Id, default)).ReturnsAsync(order);
        _numGenMock.Setup(g => g.GenerateNextAsync(default)).ReturnsAsync("INV-202603-0001");
        _storeInfoMock.Setup(s => s.GetCurrentAsync(default)).ReturnsAsync(DefaultStore());
        _invoiceRepoMock.Setup(r => r.AddAsync(It.IsAny<Invoice>(), default))
            .ReturnsAsync((Invoice inv, CancellationToken _) => inv);
        SetupTransaction();

        var result = await CreateHandler().Handle(new GenerateInvoiceCommand(order.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.InvoiceNumber.Should().Be("INV-202603-0001");
    }

    // ── Idempotency: invoice already exists ───────────────────────────────────

    [Fact]
    public async Task Handle_InvoiceAlreadyExists_ReturnsExistingInvoice()
    {
        var order = CompletedOrder();
        var existingInvoice = Invoice.CreateFromOrder(order, DefaultStore(), "INV-EXISTING");
        _invoiceRepoMock.Setup(r => r.GetByOrderIdAsync(order.Id, default)).ReturnsAsync(existingInvoice);

        var result = await CreateHandler().Handle(new GenerateInvoiceCommand(order.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.InvoiceNumber.Should().Be("INV-EXISTING");
        // Should NOT generate a new number or call the DB a second time
        _numGenMock.Verify(g => g.GenerateNextAsync(default), Times.Never);
    }
}