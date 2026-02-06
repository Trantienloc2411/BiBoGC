using AuthorizationModule.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Command.Login;

public record LoginCommand(string UserName, string Password, string IpAddress) : IRequest<Result<AuthResponseDto>>;