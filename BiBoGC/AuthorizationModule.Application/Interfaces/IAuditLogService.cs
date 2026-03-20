using AuthorizationModule.Domain.Entities;

namespace AuthorizationModule.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
