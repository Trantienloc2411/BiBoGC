using Finance.Domain.Enums;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Commands.CreateExpense;

public record CreateExpenseCommand(
    ExpenseCategory Category,
    decimal Amount,
    string Description,
    DateTime ExpenseDate,
    ExpensePaymentMethod PaymentMethod,
    string? ReceiptNumber
) : IRequest<Result<Guid>>;
