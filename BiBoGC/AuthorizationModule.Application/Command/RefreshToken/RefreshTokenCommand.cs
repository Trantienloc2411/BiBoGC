using AuthorizationModule.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace AuthorizationModule.Application.Command.RefreshToken;

public record RefreshTokenCommand(string refreshToken, string ipAddress) : IRequest<Result<AuthResponseDto>>;