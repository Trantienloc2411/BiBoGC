using AuthorizationModule.Application.Command.Login;
using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Application.Common;

namespace BiBoGC.Tests.Unit.Application;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    private LoginCommandHandler CreateHandler()
    {
        return new LoginCommandHandler(_authServiceMock.Object);
    }

    // ── Success ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithTokens()
    {
        var dto = new AuthResponseDto
        {
            AccessToken = "jwt-token",
            RefreshToken = "refresh-token",
            AccessTokenExpirationAt = DateTime.UtcNow.AddMinutes(15),
            Role = "Owner",
            UserName = "admin"
        };
        _authServiceMock
            .Setup(s => s.LoginAsync("admin", "pass123", "127.0.0.1", default))
            .ReturnsAsync(Result<AuthResponseDto>.Success(dto));

        var result = await CreateHandler().Handle(
            new LoginCommand("admin", "pass123", "127.0.0.1"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("jwt-token");
        result.Value.UserName.Should().Be("admin");
    }

    // ── Invalid credentials ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_InvalidCredentials_ReturnsFailureWithMessage()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(Result<AuthResponseDto>.Failure("Sai mật khẩu"));

        var result = await CreateHandler().Handle(
            new LoginCommand("admin", "wrong", "127.0.0.1"), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Đăng nhập không thành công"));
    }

    // ── Blocked/inactive user ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_InactiveUser_ReturnsFailure()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(Result<AuthResponseDto>.Failure("Tài khoản bị khóa"));

        var result = await CreateHandler().Handle(
            new LoginCommand("blocked", "pass", "10.0.0.1"), default);

        result.IsSuccess.Should().BeFalse();
    }

    // ── IpAddress is forwarded ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ForwardsIpAddressToAuthService()
    {
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(Result<AuthResponseDto>.Failure("err"));

        await CreateHandler().Handle(
            new LoginCommand("u", "p", "192.168.1.1"), default);

        _authServiceMock.Verify(s =>
            s.LoginAsync("u", "p", "192.168.1.1", default), Times.Once);
    }
}