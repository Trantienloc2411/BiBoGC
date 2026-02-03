using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
///TO-DO:  Update Auth Controller using mediator instead of authService.
namespace BiBoGC.Controllers;
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController (IMediator mediator): ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.LoginAsync(loginRequestDto.UserName, loginRequestDto.Password, ipAddress);
        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Đăng nhập thất bại",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });
        return Ok(result.Value);
    }
    
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Refresh token thất bại",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });

        return Ok(result.Value);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.RevokeRefreshTokenAsync(request.RefreshToken, ipAddress);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Revoke thất bại",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });

        return Ok();
    }

}