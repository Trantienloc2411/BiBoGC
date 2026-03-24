using BiBoGC.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Commands.MarkAllAsRead;
using Notification.Application.Commands.MarkAsRead;
using Notification.Application.DTOs;
using Notification.Application.Queries.GetNotifications;
using Shared.Domain.Enums;

namespace BiBoGC.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách thông báo theo vai trò và trạng thái đọc
    /// </summary>
    /// <param name="role">Admin | Seller (bỏ trống = tất cả)</param>
    /// <param name="unreadOnly">true = chỉ chưa đọc</param>
    /// <param name="page">Trang (mặc định 1)</param>
    /// <param name="pageSize">Kích thước trang (mặc định 20, tối đa 100)</param>
    [HttpGet]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<NotificationListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] string? role = null,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        NotificationRole? parsedRole = null;
        if (!string.IsNullOrWhiteSpace(role) &&
            Enum.TryParse<NotificationRole>(role, true, out var r))
            parsedRole = r;

        var result = await _mediator.Send(
            new GetNotificationsQuery(parsedRole, unreadOnly, page, pageSize), ct);

        return Ok(ApiResponse<NotificationListDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Đánh dấu một thông báo là đã đọc
    /// </summary>
    [HttpPut("{id:guid}/read")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(id), ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Error(
                result.Errors.FirstOrDefault() ?? "Không tìm thấy thông báo."));

        return Ok(ApiResponse<object>.Ok(null, "Đã đánh dấu thông báo là đã đọc."));
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo của một vai trò là đã đọc
    /// </summary>
    /// <param name="role">Admin | Seller | Both</param>
    [HttpPut("read-all")]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkAllAsRead(
        [FromQuery] string role = "Admin",
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<NotificationRole>(role, true, out var parsedRole))
            return BadRequest(ApiResponse<object>.Error($"Vai trò '{role}' không hợp lệ."));

        await _mediator.Send(new MarkAllAsReadCommand(parsedRole), ct);
        return Ok(ApiResponse<object>.Ok(null, "Đã đánh dấu tất cả thông báo là đã đọc."));
    }
}