using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using AuthorizationModule.Domain.Entities;
using AuthorizationModule.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Common;

namespace AuthorizationModule.Infrastructure.Services;

public class AuthService(AuthorizationDbContext dbContext, IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{

    public async Task<Result<AuthResponseDto>> LoginAsync(string username, string password, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == username && u.IsActive,  cancellationToken);

        if (user is null || !passwordHasher.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            return Result<AuthResponseDto>.Failure("Tài khoản hoặc mật khẩu không chính xác");
        
        var accessToken = jwtTokenGenerator.GenerateJwtToken(user, out var accessExpirationAt);
        var refreshTokenValue = jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ExpireAt = DateTime.UtcNow.AddDays(7),
        };
        
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpirationAt = accessExpirationAt,
            Role = user.Role.ToString(),
            UserName = user.Username,
        });
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var token = await dbContext.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token is null || !token.IsActive)
            return Result<AuthResponseDto>.Failure("Refresh token không hợp lệ");

        var user = token.User;
        var accessToken = jwtTokenGenerator.GenerateJwtToken(user, out var accessExpirationAt);
        var newRefreshTokenValue = jwtTokenGenerator.GenerateRefreshToken();
        
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = newRefreshTokenValue;

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ExpireAt = DateTime.UtcNow.AddDays(7),
        };
        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpirationAt = accessExpirationAt,
            Role = user.Role.ToString(),
            UserName = user.Username,
        });
    }

    public async Task<Result> RevokeRefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var token = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (token is null || !token.IsActive)
            return Result.Failure("Refresh token không thành công, vui lòng sử dụng token mới");
        
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}