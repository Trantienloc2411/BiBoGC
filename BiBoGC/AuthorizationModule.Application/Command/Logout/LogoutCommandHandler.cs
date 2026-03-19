using AuthorizationModule.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Command.Logout;

public class LogoutCommandHandler(IAuthService authService) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await authService.LogoutAsync(request.UserId, request.IpAddress, cancellationToken);
    }
}