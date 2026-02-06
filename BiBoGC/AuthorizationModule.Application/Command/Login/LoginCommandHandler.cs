using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Command.Login;

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.UserName, request.Password, request.IpAddress, cancellationToken);
        if (!result.IsSuccess)
            return Result<AuthResponseDto>.Failure(
                $"Đăng nhập không thành công. Lỗi chi tiết {result.Errors.FirstOrDefault()}");
        else
            return result;
    }
}