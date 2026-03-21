using AuthorizationModule.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Queries.GetAuditLogs;

public record GetAuditLogsQuery : IRequest<PagedResult<AuditLogDto>>
{
    public Guid? UserId { get; init; }
    public string? Action { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
