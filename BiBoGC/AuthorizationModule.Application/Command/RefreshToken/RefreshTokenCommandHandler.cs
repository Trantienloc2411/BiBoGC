using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Command.RefreshToken;

public class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(request.refreshToken, request.ipAddress, cancellationToken);
        return !result.IsSuccess ? Result<AuthResponseDto>.Failure(result.Errors) : result;
    }
}