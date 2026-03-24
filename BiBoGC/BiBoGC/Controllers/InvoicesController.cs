using BiBoGC.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sale.Application.DTOs;
using Sale.Application.Queries.ExportInvoicePdf;
using Sale.Application.Queries.GetInvoice;
using Sale.Application.Queries.GetInvoices;
using Shared.Application.Common;

namespace BiBoGC.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách hóa đơn
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        CancellationToken ct = default)
    {
        var query = new GetInvoicesQuery(page, pageSize, dateFrom, dateTo);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<PagedResult<InvoiceDto>>.Ok(result.Value!));
    }

    /// <summary>
    /// Lấy chi tiết hóa đơn
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken ct = default)
    {
        var query = new GetInvoiceQuery(id);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<InvoiceDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Xuất hóa đơn dạng PDF
    /// </summary>
    [HttpGet("{id:guid}/export/pdf")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportInvoicePdf(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ExportInvoicePdfQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định"));

        return File(result.Value!, "application/pdf", $"invoice-{id}.pdf");
    }
}