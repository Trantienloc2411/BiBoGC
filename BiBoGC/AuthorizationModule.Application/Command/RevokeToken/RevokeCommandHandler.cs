using AuthorizationModule.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Command.RevokeToken;

public class RevokeCommandHandler(IAuthService authService) : IRequestHandler<RevokeCommand, Result>
{
    public async Task<Result> Handle(RevokeCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.RevokeRefreshTokenAsync(request.RefreshToken, request.IpAddress, cancellationToken);
        return result.IsSuccess ? result : Result.Failure(result.Errors);
    }
}