namespace AuthorizationModule.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Null for failed logins (user not found) or unauthenticated attempts.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Stored at log time so it survives user renames/deletions.</summary>
    public string? Username { get; set; }

    /// <summary>E.g. "Login", "Logout", "API_Access".</summary>
    public string Action { get; set; } = string.Empty;

    public string? HttpMethod { get; set; }
    public string? Endpoint { get; set; }
    public int? StatusCode { get; set; }
    public string? IpAddress { get; set; }
    public string? Description { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
