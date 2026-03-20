using AuthorizationModule.Application.Interfaces;
using AuthorizationModule.Domain.Entities;
using AuthorizationModule.Infrastructure.Data;
using Microsoft.Extensions.Logging;

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
}
