using BiBoGC.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sale.Application.Commands.AddItemToOrder;
using Sale.Application.Commands.ApplyDiscount;
using Sale.Application.Commands.CancelOrder;
using Sale.Application.Commands.CompleteOrder;
using Sale.Application.Commands.CreateSalesOrder;
using Sale.Application.Commands.GenerateInvoice;
using Sale.Application.Commands.RemoveItemFromOrder;
using Sale.Application.Commands.UpdateOrderItemQuantity;
using Sale.Application.DTOs;
using Sale.Application.Queries.GetSalesOrder;
using Sale.Application.Queries.GetSalesOrders;
using Sale.Domain.Enum;
using Shared.Application.Common;

namespace BiBoGC.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SalesOrdersController : ControllerBase
{
    private readonly ILogger<SalesOrdersController> _logger;
    private readonly IMediator _mediator;

    public SalesOrdersController(IMediator mediator, ILogger<SalesOrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách đơn hàng
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SalesOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = new GetSalesOrdersQuery(page, pageSize, status, dateFrom, dateTo, search);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<PagedResult<SalesOrderDto>>.Ok(result.Value!));
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct = default)
    {
        var query = new GetSalesOrderQuery(id);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Không tìm thấy",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Tạo đơn hàng mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateSalesOrderRequest request,
        CancellationToken ct = default)
    {
        var command = new CreateSalesOrderCommand(
            PaymentMethod: request.PaymentMethod,
            CustomerName: request.CustomerName,
            CustomerPhone: request.CustomerPhone,
            Notes: request.Notes
        );

        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = result.Value!.Id },
            ApiResponse<SalesOrderDto>.Ok(result.Value));
    }

    /// <summary>
    /// Thêm sản phẩm vào đơn hàng (theo ProductVariant)
    /// </summary>
    [HttpPost("{id:guid}/items")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] AddItemRequest request,
        CancellationToken ct = default)
    {
        var command = new AddItemToOrderCommand(
            OrderId: id,
            ProductId: request.ProductId,
            ProductVariantId: request.ProductVariantId,
            ProductBatchId: request.ProductBatchId,
            Quantity: request.Quantity
        );

        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Cập nhật số lượng sản phẩm
    /// </summary>
    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateItemQuantity(
        Guid id,
        Guid itemId,
        [FromBody] UpdateItemQuantityRequest request,
        CancellationToken ct = default)
    {
        var command = new UpdateOrderItemQuantityCommand(id, itemId, request.Quantity);
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Xóa sản phẩm khỏi đơn hàng
    /// </summary>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveItem(
        Guid id,
        Guid itemId,
        CancellationToken ct = default)
    {
        var command = new RemoveItemFromOrderCommand(id, itemId);
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Áp dụng giảm giá (theo số tiền)
    /// </summary>
    [HttpPost("{id:guid}/discount")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyDiscount(
        Guid id,
        [FromBody] ApplyDiscountRequest request,
        CancellationToken ct = default)
    {
        var command = new ApplyDiscountCommand(id, request.DiscountAmount);
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Hoàn thành đơn hàng (trừ kho)
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteOrder(
        Guid id,
        [FromBody] CompleteOrderRequest request,
        CancellationToken ct = default)
    {
        var command = new CompleteOrderCommand(id, request.AmountPaid);
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("CompleteOrder failed for order {OrderId}: {Errors}",
                id, string.Join("; ", result.Errors));
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));
        }

        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Hủy đơn hàng
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequest? request = null,
        CancellationToken ct = default)
    {
        var command = new CancelOrderCommand(id, request?.Reason);
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Xuất hóa đơn từ đơn hàng
    /// </summary>
    [HttpPost("{id:guid}/invoice")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateInvoice(
        Guid id,
        CancellationToken ct = default)
    {
        var command = new GenerateInvoiceCommand(id);
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Errors.FirstOrDefault() ?? "Lỗi không xác định",
                result.Errors.Skip(1)));

        return Ok(ApiResponse<InvoiceDto>.Ok(result.Value!));
    }
}

#region Request DTOs

public record CreateSalesOrderRequest(
    PaymentMethod PaymentMethod,
    string? CustomerName = null,
    string? CustomerPhone = null,
    string? Notes = null
);

public record AddItemRequest(
    Guid ProductId,
    Guid ProductVariantId,
    Guid? ProductBatchId = null,
    int Quantity = 1
);

public record UpdateItemQuantityRequest(int Quantity);

public record ApplyDiscountRequest(decimal DiscountAmount);

public record CompleteOrderRequest(decimal AmountPaid);

public record CancelOrderRequest(string? Reason = null);

#endregion