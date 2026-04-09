using BiBoGC.Models;
using Finance.Application.Commands.CreateExpense;
using Finance.Application.Commands.UpdateTaxConfig;
using Finance.Application.DTOs;
using Finance.Application.Queries.ExportDailySalesReportPdf;
using Finance.Application.Queries.ExportTaxDeclaration;
using Finance.Application.Queries.ExportTaxReport;
using Shared.Application.Common;
using Finance.Application.Queries.ExportFinancialReportPdf;
using Finance.Application.Queries.ExportMonthlySalesReportPdf;
using Finance.Application.Queries.GetAnnualRevenueReport;
using Finance.Application.Queries.GetDailyExpenseSummary;
using Finance.Application.Queries.GetDailySalesReport;
using Finance.Application.Queries.GetMonthlyFinancialReport;
using Finance.Application.Queries.GetMonthlySalesReport;
using Finance.Application.Queries.GetTaxConfig;
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

    // ─── Tax Configuration ───────────────────────────────────────────────────

    /// <summary>
    /// Lấy cấu hình thuế VAT và PIT hiện tại
    /// </summary>
    [HttpGet("tax-config")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<TaxConfigsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxConfig(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTaxConfigQuery(), ct);
        return Ok(ApiResponse<TaxConfigsDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Cập nhật cấu hình thuế (VAT hoặc PIT)
    /// </summary>
    /// <remarks>taxType: "VAT" hoặc "PIT"</remarks>
    [HttpPut("tax-config")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<TaxConfigDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTaxConfig(
        [FromBody] UpdateTaxConfigRequest request,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new UpdateTaxConfigCommand(request.TaxType, request.Rate, request.IsEnabled), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<TaxConfigDto>.Ok(result.Value!,
            $"Cập nhật cấu hình thuế {request.TaxType.ToUpper()} thành công."));
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
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        if (targetMonth is < 1 or > 12)
            return BadRequest(ApiResponse<object>.Error("Tháng không hợp lệ. Giá trị hợp lệ: 1-12."));

        var result = await _mediator.Send(new GetMonthlySalesReportQuery(targetYear, targetMonth), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return Ok(ApiResponse<MonthlySalesReportDto>.Ok(result.Value!));
    }

    // ─── Annual Revenue Report ───────────────────────────────────────────────

    /// <summary>
    /// Báo cáo doanh thu theo năm (phân tích 12 tháng)
    /// </summary>
    [HttpGet("reports/sales/annual")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<AnnualRevenueReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnnualRevenueReport(
        [FromQuery] int? year = null,
        CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var result = await _mediator.Send(new GetAnnualRevenueReportQuery(targetYear), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return Ok(ApiResponse<AnnualRevenueReportDto>.Ok(result.Value!));
    }

    // ─── Report PDF Exports ──────────────────────────────────────────────────

    /// <summary>
    /// Xuất báo cáo doanh thu ngày dạng PDF
    /// </summary>
    [HttpGet("reports/sales/daily/export/pdf")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportDailySalesReportPdf(
        [FromQuery] DateOnly? date = null,
        CancellationToken ct = default)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _mediator.Send(new ExportDailySalesReportPdfQuery(targetDate), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return File(result.Value!, "application/pdf",
            $"daily-sales-report-{targetDate:yyyy-MM-dd}.pdf");
    }

    /// <summary>
    /// Xuất báo cáo doanh thu tháng dạng PDF
    /// </summary>
    [HttpGet("reports/sales/monthly/export/pdf")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportMonthlySalesReportPdf(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        if (targetMonth is < 1 or > 12)
            return BadRequest(ApiResponse<object>.Error("Tháng không hợp lệ. Giá trị hợp lệ: 1-12."));

        var result = await _mediator.Send(new ExportMonthlySalesReportPdfQuery(targetYear, targetMonth), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return File(result.Value!, "application/pdf",
            $"monthly-sales-report-{targetYear}-{targetMonth:D2}.pdf");
    }

    /// <summary>
    /// Xuất báo cáo tài chính tháng dạng PDF
    /// </summary>
    [HttpGet("reports/financial/monthly/export/pdf")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportFinancialReportPdf(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        if (targetMonth is < 1 or > 12)
            return BadRequest(ApiResponse<object>.Error("Tháng không hợp lệ. Giá trị hợp lệ: 1-12."));

        var result = await _mediator.Send(new ExportFinancialReportPdfQuery(targetYear, targetMonth), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return File(result.Value!, "application/pdf",
            $"financial-report-{targetYear}-{targetMonth:D2}.pdf");
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
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        if (targetMonth is < 1 or > 12)
            return BadRequest(ApiResponse<object>.Error("Tháng không hợp lệ. Giá trị hợp lệ: 1-12."));

        var result = await _mediator.Send(new GetMonthlyFinancialReportQuery(targetYear, targetMonth), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return Ok(ApiResponse<MonthlyFinancialReportDto>.Ok(result.Value!));
    }

    // ─── Tax Report Export ───────────────────────────────────────────────────

    /// <summary>
    /// Xuất bảng kê hoá đơn VAT dạng Excel (dùng để nộp thuế)
    /// ?year=2026&amp;month=3 → tháng 3/2026
    /// ?year=2026           → cả năm 2026
    /// </summary>
    [HttpGet("reports/tax/export")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportTaxReport(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        if (month.HasValue && month is < 1 or > 12)
            return BadRequest(ApiResponse<object>.Error("Tháng không hợp lệ. Giá trị hợp lệ: 1-12."));

        var result = await _mediator.Send(new ExportTaxReportQuery(targetYear, month), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        var export = result.Value!;
        if (export.IsZip)
        {
            var zipName = month.HasValue
                ? $"S2a-HKD-{targetYear}-T{month:D2}.zip"
                : $"S2a-HKD-{targetYear}.zip";
            return File(export.Data, "application/zip", zipName);
        }

        var fileName = month.HasValue
            ? $"S2a-HKD-{targetYear}-T{month:D2}.xlsx"
            : $"S2a-HKD-{targetYear}.xlsx";

        return File(export.Data,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// Xuất tờ khai thuế Mẫu 01/CNKD dạng Word (.docx) theo TT 40/2021/TT-BTC
    /// ?year=2026&amp;monthFrom=1&amp;monthTo=12
    /// </summary>
    [HttpGet("reports/tax/declaration/export")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportTaxDeclaration(
        [FromQuery] int? year = null,
        [FromQuery] int monthFrom = 1,
        [FromQuery] int monthTo = 12,
        CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        var result = await _mediator.Send(
            new ExportTaxDeclarationQuery(targetYear, monthFrom, monthTo), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        var fileName = $"to-khai-thue-01-CNKD-{targetYear}-T{monthFrom:D2}-T{monthTo:D2}.docx";
        return File(result.Value!,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            fileName);
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

/// <param name="TaxType">"VAT" hoặc "PIT"</param>
public record UpdateTaxConfigRequest(string TaxType, decimal Rate, bool IsEnabled);

// Keep this at the end — Tax Report endpoint is in FinanceController above