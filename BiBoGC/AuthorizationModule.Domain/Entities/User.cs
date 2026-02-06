using AuthorizationModule.Domain.Enums;

namespace AuthorizationModule.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

}