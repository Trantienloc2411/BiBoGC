using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Queries.GetAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Common;

namespace BiBoGC.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Administrator")]
public class AuditLogsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns a paged list of audit log entries. Administrator only.
    /// </summary>
    /// <param name="userId">Filter by specific user ID.</param>
    /// <param name="action">Filter by action prefix, e.g. "Login", "Product", "SalesOrder.Complete".</param>
    /// <param name="from">Start of time range (UTC).</param>
    /// <param name="to">End of time range (UTC).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page, max 100 (default: 50).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery
        {
            UserId   = userId,
            Action   = action,
            From     = from,
            To       = to,
            Page     = page,
            PageSize = pageSize
        };

        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
