using Finance.Application.Interfaces;
using Finance.Domain.Entities;
using MediatR;
using Shared.Application.Common;
using Shared.Application.Interfaces;

namespace Finance.Application.Commands.CreateExpense;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<Guid>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IFinanceUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;

    public CreateExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        IFinanceUnitOfWork unitOfWork,
        IAuditLogger auditLogger)
    {
        _expenseRepository = expenseRepository;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = Expense.Create(
            category: request.Category,
            amount: request.Amount,
            description: request.Description,
            expenseDate: request.ExpenseDate,
            paymentMethod: request.PaymentMethod,
            receiptNumber: request.ReceiptNumber);

        await _expenseRepository.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "Expense.Create",
            isSuccess: true,
            description: $"Category={request.Category}, Amount={request.Amount:F2}, Date={request.ExpenseDate:yyyy-MM-dd}, Payment={request.PaymentMethod}",
            cancellationToken: cancellationToken);

        return Result<Guid>.Success(expense.Id);
    }
}
