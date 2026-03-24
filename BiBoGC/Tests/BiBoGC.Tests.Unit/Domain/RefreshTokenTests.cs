using AuthorizationModule.Domain.Entities;
using FluentAssertions;

namespace BiBoGC.Tests.Unit.Domain;

public class RefreshTokenTests
{
    private static RefreshToken BuildToken(
        DateTime? expireAt = null,
        DateTime? revokedAt = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "tok-abc",
            ExpireAt = expireAt ?? DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = "127.0.0.1",
            RevokedAt = revokedAt,
            UserId = Guid.NewGuid()
        };
    }

    // ── IsExpired ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsExpired_FutureExpiry_ReturnsFalse()
    {
        var token = BuildToken(DateTime.UtcNow.AddDays(1));
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_PastExpiry_ReturnsTrue()
    {
        var token = BuildToken(DateTime.UtcNow.AddDays(-1));
        token.IsExpired.Should().BeTrue();
    }

    // ── IsRevoked ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsRevoked_NullRevokedAt_ReturnsFalse()
    {
        var token = BuildToken(revokedAt: null);
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void IsRevoked_SetRevokedAt_ReturnsTrue()
    {
        var token = BuildToken(revokedAt: DateTime.UtcNow.AddHours(-1));
        token.IsRevoked.Should().BeTrue();
    }

    // ── IsActive ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsActive_NotRevokedNotExpired_ReturnsTrue()
    {
        var token = BuildToken(DateTime.UtcNow.AddDays(7), null);
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_Expired_ReturnsFalse()
    {
        var token = BuildToken(DateTime.UtcNow.AddDays(-1), null);
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_Revoked_ReturnsFalse()
    {
        var token = BuildToken(DateTime.UtcNow.AddDays(7), DateTime.UtcNow);
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_RevokedAndExpired_ReturnsFalse()
    {
        var token = BuildToken(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2));
        token.IsActive.Should().BeFalse();
    }
}