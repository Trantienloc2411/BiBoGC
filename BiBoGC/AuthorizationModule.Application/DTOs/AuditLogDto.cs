namespace AuthorizationModule.Application.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? HttpMethod { get; set; }
    public string? Endpoint { get; set; }
    public int? StatusCode { get; set; }
    public string? IpAddress { get; set; }
    public string? Description { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime Timestamp { get; set; }
}
