using AuthorizationModule.Application.Command.Login;
using AuthorizationModule.Application.Command.RefreshToken;
using AuthorizationModule.Application.Command.RevokeToken;
using AuthorizationModule.Application.DTOs;
using AuthorizationModule.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BiBoGC.Controllers;
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var request = new LoginCommand(loginRequestDto.UserName, loginRequestDto.Password, ipAddress);
        var result = await _mediator.Send(request);
        
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
        
        var command = new RefreshTokenCommand(request.RefreshToken, ipAddress);
        var result = await _mediator.Send(command);
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
        var command = new RevokeCommand(request.RefreshToken, ipAddress);
        var result = await _mediator.Send(command);
        
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