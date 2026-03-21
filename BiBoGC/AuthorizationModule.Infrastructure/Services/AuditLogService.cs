using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using AuthorizationModule.Application.Queries.GetAuditLogs;
using AuthorizationModule.Domain.Entities;
using AuthorizationModule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Common;

namespace AuthorizationModule.Infrastructure.Services;

public class AuditLogService(
    AuthorizationDbContext dbContext,
    ILogger<AuditLogService> logger) : IAuditLogService
{
    public async Task LogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        try
        {
            dbContext.AuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Audit log failure must never break the main operation
            logger.LogError(ex, "Failed to write audit log. Action={Action} User={Username}", auditLog.Action, auditLog.Username);
        }
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(
        GetAuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page     = Math.Max(query.Page, 1);

        var q = dbContext.AuditLogs.AsNoTracking();

        if (query.UserId.HasValue)
            q = q.Where(a => a.UserId == query.UserId.Value);

        if (!string.IsNullOrWhiteSpace(query.Action))
            q = q.Where(a => a.Action.StartsWith(query.Action));

        if (query.From.HasValue)
            q = q.Where(a => a.Timestamp >= query.From.Value);

        if (query.To.HasValue)
            q = q.Where(a => a.Timestamp <= query.To.Value);

        var totalCount = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id          = a.Id,
                UserId      = a.UserId,
                Username    = a.Username,
                Action      = a.Action,
                HttpMethod  = a.HttpMethod,
                Endpoint    = a.Endpoint,
                StatusCode  = a.StatusCode,
                IpAddress   = a.IpAddress,
                Description = a.Description,
                IsSuccess   = a.IsSuccess,
                Timestamp   = a.Timestamp
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }
}
