using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Command.Logout;

public record LogoutCommand(Guid UserId, string IpAddress) : IRequest<Result>;