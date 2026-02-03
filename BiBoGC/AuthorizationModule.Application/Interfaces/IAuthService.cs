using AuthorizationModule.Application.DTOs;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(string username, string password, string ipAddress);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<Result> RevokeRefreshTokenAsync (string refreshToken, string ipAddress);
}