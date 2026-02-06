namespace AuthorizationModule.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpireAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public string CreatedByIp { get; set; } = null!;
    
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }    
    
    public bool IsExpired  => DateTime.UtcNow >= ExpireAt;
    public bool IsActive  => !IsRevoked && !IsExpired;
    public bool IsRevoked  => RevokedAt != null;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

}