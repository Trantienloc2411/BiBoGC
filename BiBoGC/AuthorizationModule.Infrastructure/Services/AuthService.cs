using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using AuthorizationModule.Domain.Entities;
using AuthorizationModule.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Common;

namespace AuthorizationModule.Infrastructure.Services;

public class AuthService(
    AuthorizationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IAuditLogService auditLogService) : IAuthService
{
    public async Task<Result<AuthResponseDto>> LoginAsync(string username, string password, string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken);

        if (user is null || !passwordHasher.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        {
            await auditLogService.LogAsync(new AuditLog
            {
                Username = username,
                Action = "Login",
                IpAddress = ipAddress,
                IsSuccess = false,
                Description = "Tài khoản hoặc mật khẩu không chính xác",
            }, cancellationToken);
            return Result<AuthResponseDto>.Failure("Tài khoản hoặc mật khẩu không chính xác");
        }

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

        await auditLogService.LogAsync(new AuditLog
        {
            UserId = user.Id,
            Username = user.Username,
            Action = "Login",
            IpAddress = ipAddress,
            IsSuccess = true,
        }, cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpirationAt = accessExpirationAt,
            Role = user.Role.ToString(),
            UserName = user.Username,
        });
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var token = await dbContext.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token is null || !token.IsActive)
        {
            await auditLogService.LogAsync(new AuditLog
            {
                Action = "TokenRefresh",
                IpAddress = ipAddress,
                IsSuccess = false,
                Description = "Refresh token không hợp lệ hoặc đã hết hạn",
            }, cancellationToken);
            return Result<AuthResponseDto>.Failure("Refresh token không hợp lệ");
        }

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

        await auditLogService.LogAsync(new AuditLog
        {
            UserId = user.Id,
            Username = user.Username,
            Action = "TokenRefresh",
            IpAddress = ipAddress,
            IsSuccess = true,
        }, cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpirationAt = accessExpirationAt,
            Role = user.Role.ToString(),
            UserName = user.Username,
        });
    }

    public async Task<Result> RevokeRefreshTokenAsync(string refreshToken, string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var token = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (token is null || !token.IsActive)
        {
            await auditLogService.LogAsync(new AuditLog
            {
                Action = "TokenRevoke",
                IpAddress = ipAddress,
                IsSuccess = false,
                Description = "Refresh token không hợp lệ hoặc đã bị thu hồi",
            }, cancellationToken);
            return Result.Failure("Refresh token không thành công, vui lòng sử dụng token mới");
        }

        var revokedUser = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == token.UserId)
            .Select(u => new { u.Id, u.Username })
            .FirstOrDefaultAsync(cancellationToken);

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(new AuditLog
        {
            UserId = revokedUser?.Id,
            Username = revokedUser?.Username,
            Action = "TokenRevoke",
            IpAddress = ipAddress,
            IsSuccess = true,
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> LogoutAsync(Guid userId, string ipAddress, CancellationToken cancellationToken = default)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null && r.ExpireAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(cancellationToken);

        await auditLogService.LogAsync(new AuditLog
        {
            UserId = userId,
            Username = user,
            Action = "Logout",
            IpAddress = ipAddress,
            IsSuccess = true,
        }, cancellationToken);

        return Result.Success();
    }
}