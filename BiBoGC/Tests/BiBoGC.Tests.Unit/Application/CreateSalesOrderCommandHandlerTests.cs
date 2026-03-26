using FluentAssertions;
using Moq;
using Sale.Application.Commands.CreateSalesOrder;
using Sale.Application.Interfaces;
using Sale.Domain.Enum;

namespace BiBoGC.Tests.Unit.Application;

public class CreateSalesOrderCommandHandlerTests
{
    private readonly Mock<IOrderNumberGenerator> _generatorMock = new();
    private readonly Mock<ISalesOrderRepository> _orderRepoMock = new();

    private CreateSalesOrderCommandHandler CreateHandler()
    {
        return new CreateSalesOrderCommandHandler(_orderRepoMock.Object, _generatorMock.Object);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithOrderNumber()
    {
        _generatorMock
            .Setup(g => g.GenerateNextAsync(default))
            .ReturnsAsync("SO-20260324-0001");

        var result = await CreateHandler().Handle(
            new CreateSalesOrderCommand(PaymentMethod.Cash), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.OrderNumber.Should().Be("SO-20260324-0001");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAdd()
    {
        _generatorMock.Setup(g => g.GenerateNextAsync(default)).ReturnsAsync("SO-001");

        await CreateHandler().Handle(new CreateSalesOrderCommand(PaymentMethod.Cash), default);

        _orderRepoMock.Verify(r => r.AddAsync(
            It.Is<Sale.Domain.Domain.SalesOrder>(o => o.OrderNumber == "SO-001"),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomerInfo_OrderHasCustomerData()
    {
        _generatorMock.Setup(g => g.GenerateNextAsync(default)).ReturnsAsync("SO-002");

        var result = await CreateHandler().Handle(
            new CreateSalesOrderCommand(
                PaymentMethod.QRPayment,
                "Nguyễn Văn A",
                "0901234567"),
            default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CustomerName.Should().Be("Nguyễn Văn A");
        result.Value.CustomerPhone.Should().Be("0901234567");
    }

    [Fact]
    public async Task Handle_OrderCreated_StatusIsDraft()
    {
        _generatorMock.Setup(g => g.GenerateNextAsync(default)).ReturnsAsync("SO-003");

        var result = await CreateHandler().Handle(
            new CreateSalesOrderCommand(PaymentMethod.Cash), default);

        result.Value!.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task Handle_OrderCreated_SubTotalIsZero()
    {
        _generatorMock.Setup(g => g.GenerateNextAsync(default)).ReturnsAsync("SO-004");

        var result = await CreateHandler().Handle(
            new CreateSalesOrderCommand(PaymentMethod.Cash), default);

        result.Value!.SubTotal.Should().Be(0);
        result.Value.TotalAmount.Should().Be(0);
    }
}