using BiBoGC.Models;
using Finance.Application.Commands.CreateExpense;
using Finance.Application.DTOs;
using Finance.Application.Queries.GetDailyExpenseSummary;
using Finance.Application.Queries.GetDailySalesReport;
using Finance.Application.Queries.GetMonthlyFinancialReport;
using Finance.Application.Queries.GetMonthlySalesReport;
using Finance.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiBoGC.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FinanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ─── Expenses ────────────────────────────────────────────────────────────

    /// <summary>
    /// Ghi nhận chi phí mới
    /// </summary>
    [HttpPost("expenses")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExpense(
        [FromBody] CreateExpenseRequest request,
        CancellationToken ct = default)
    {
        var command = new CreateExpenseCommand(
            request.Category,
            request.Amount,
            request.Description,
            request.ExpenseDate,
            request.PaymentMethod,
            request.ReceiptNumber);

        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Created(
            $"/api/finance/expenses/{result.Value}",
            ApiResponse<Guid>.Ok(result.Value, "Ghi nhận chi phí thành công."));
    }

    /// <summary>
    /// Tổng kết chi phí theo ngày
    /// </summary>
    [HttpGet("expenses/daily")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<DailyExpenseSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyExpenseSummary(
        [FromQuery] DateOnly? date = null,
        CancellationToken ct = default)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _mediator.Send(new GetDailyExpenseSummaryQuery(targetDate), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return Ok(ApiResponse<DailyExpenseSummaryDto>.Ok(result.Value!));
    }

    // ─── Sales Reports ───────────────────────────────────────────────────────

    /// <summary>
    /// Báo cáo doanh thu theo ngày
    /// </summary>
    [HttpGet("reports/sales/daily")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<DailySalesReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySalesReport(
        [FromQuery] DateOnly? date = null,
        CancellationToken ct = default)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _mediator.Send(new GetDailySalesReportQuery(targetDate), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return Ok(ApiResponse<DailySalesReportDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Báo cáo doanh thu theo tháng
    /// </summary>
    [HttpGet("reports/sales/monthly")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<MonthlySalesReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlySalesReport(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var targetYear  = year  ?? now.Year;
        var targetMonth = month ?? now.Month;

        if (targetMonth is < 1 or > 12)
            return BadRequest(ApiResponse<object>.Error("Tháng không hợp lệ. Giá trị hợp lệ: 1-12."));

        var result = await _mediator.Send(new GetMonthlySalesReportQuery(targetYear, targetMonth), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return Ok(ApiResponse<MonthlySalesReportDto>.Ok(result.Value!));
    }

    // ─── Financial Report ────────────────────────────────────────────────────

    /// <summary>
    /// Báo cáo tài chính tháng (doanh thu, COGS, chi phí, lợi nhuận)
    /// </summary>
    [HttpGet("reports/financial/monthly")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<MonthlyFinancialReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyFinancialReport(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var targetYear  = year  ?? now.Year;
        var targetMonth = month ?? now.Month;

        if (targetMonth is < 1 or > 12)
            return BadRequest(ApiResponse<object>.Error("Tháng không hợp lệ. Giá trị hợp lệ: 1-12."));

        var result = await _mediator.Send(new GetMonthlyFinancialReportQuery(targetYear, targetMonth), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return Ok(ApiResponse<MonthlyFinancialReportDto>.Ok(result.Value!));
    }
}

// ─── Request models ──────────────────────────────────────────────────────────

public record CreateExpenseRequest(
    ExpenseCategory Category,
    decimal Amount,
    string Description,
    DateTime ExpenseDate,
    ExpensePaymentMethod PaymentMethod,
    string? ReceiptNumber);
