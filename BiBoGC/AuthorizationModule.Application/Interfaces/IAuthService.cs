using AuthorizationModule.Application.DTOs;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(string username, string password, string ipAddress,
        CancellationToken cancellationToken);

    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress,
        CancellationToken cancellationToken);

    Task<Result> RevokeRefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(Guid userId, string ipAddress, CancellationToken cancellationToken);
}