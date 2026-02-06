using AuthorizationModule.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Command.RevokeToken;

public record RevokeCommand(string RefreshToken, string IpAddress) : IRequest<Result>;
