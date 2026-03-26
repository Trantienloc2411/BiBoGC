using Finance.Application.Commands.CreateExpense;
using Finance.Application.Interfaces;
using Finance.Domain.Enums;
using FluentAssertions;
using Moq;
using Shared.Application.Interfaces;

namespace BiBoGC.Tests.Unit.Application;

public class CreateExpenseCommandHandlerTests
{
    private readonly Mock<IAuditLogger> _auditMock = new();
    private readonly Mock<IExpenseRepository> _repoMock = new();
    private readonly Mock<IFinanceUnitOfWork> _uowMock = new();

    private CreateExpenseCommandHandler CreateHandler()
    {
        return new CreateExpenseCommandHandler(_repoMock.Object, _uowMock.Object, _auditMock.Object);
    }

    private static CreateExpenseCommand ValidCommand()
    {
        return new CreateExpenseCommand(
            ExpenseCategory.Utilities,
            150_000m,
            "Tiền điện",
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpensePaymentMethod.Cash,
            "HD-001");
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsExpenseId()
    {
        var result = await CreateHandler().Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsExpenseToRepository()
    {
        await CreateHandler().Handle(ValidCommand(), default);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<Finance.Domain.Entities.Expense>(e =>
                e.Amount == 150_000m &&
                e.Category == ExpenseCategory.Utilities),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        await CreateHandler().Handle(ValidCommand(), default);

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_WritesAuditLog()
    {
        await CreateHandler().Handle(ValidCommand(), default);

        _auditMock.Verify(a => a.LogAsync(
            "Expense.Create", true,
            null, null,
            It.Is<string?>(d => d != null && d.Contains("150000")),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_ZeroAmount_ThrowsFromDomain()
    {
        var cmd = ValidCommand() with { Amount = 0m };

        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("amount");
    }

    [Fact]
    public async Task Handle_NegativeAmount_ThrowsFromDomain()
    {
        var cmd = ValidCommand() with { Amount = -1m };

        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("amount");
    }

    [Fact]
    public async Task Handle_BlankDescription_ThrowsFromDomain()
    {
        var cmd = ValidCommand() with { Description = "   " };

        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("description");
    }
}