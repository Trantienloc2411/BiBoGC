using BiBoGC.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sale.Application.DTOs;
using Sale.Application.Queries.GetInvoice;
using Sale.Application.Queries.GetInvoices;
using Shared.Application.Common;

namespace BiBoGC.Controllers;

[ApiController]
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
}