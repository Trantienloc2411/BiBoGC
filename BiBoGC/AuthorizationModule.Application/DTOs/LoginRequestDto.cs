namespace AuthorizationModule.Application.DTOs;

public class LoginRequestDto
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}