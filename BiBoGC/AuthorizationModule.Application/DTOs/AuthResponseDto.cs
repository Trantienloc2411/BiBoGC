namespace AuthorizationModule.Application.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessTokenExpirationAt { get; set; }
    public string Role { get; set; } = null!;
    public string UserName { get; set; } = null!;

}