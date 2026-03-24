using AuthorizationModule.Application.Command.RefreshToken;
using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Application.Common;

namespace BiBoGC.Tests.Unit.Application;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    private RefreshTokenCommandHandler CreateHandler()
    {
        return new RefreshTokenCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        var dto = new AuthResponseDto
        {
            AccessToken = "new-jwt",
            RefreshToken = "new-refresh",
            AccessTokenExpirationAt = DateTime.UtcNow.AddMinutes(15),
            Role = "Owner",
            UserName = "admin"
        };
        _authServiceMock
            .Setup(s => s.RefreshTokenAsync("old-refresh", "127.0.0.1", default))
            .ReturnsAsync(Result<AuthResponseDto>.Success(dto));

        var result = await CreateHandler().Handle(
            new RefreshTokenCommand("old-refresh", "127.0.0.1"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("new-jwt");
        result.Value.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task Handle_ExpiredRefreshToken_ReturnsFailure()
    {
        _authServiceMock
            .Setup(s => s.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(Result<AuthResponseDto>.Failure("Token đã hết hạn"));

        var result = await CreateHandler().Handle(
            new RefreshTokenCommand("expired-token", "127.0.0.1"), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_RevokedRefreshToken_ReturnsFailure()
    {
        _authServiceMock
            .Setup(s => s.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(Result<AuthResponseDto>.Failure("Token đã bị thu hồi"));

        var result = await CreateHandler().Handle(
            new RefreshTokenCommand("revoked-token", "10.0.0.1"), default);

        result.IsSuccess.Should().BeFalse();
    }
}