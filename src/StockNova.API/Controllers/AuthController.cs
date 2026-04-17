using Microsoft.AspNetCore.Mvc;
using StockNova.Application.DTOs.Auth;
using StockNova.Application.DTOs.Common;
using StockNova.Application.Interfaces;

namespace StockNova.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<AuthResponse>.Fail(result.Error!));

        return Ok(ApiResponse<AuthResponse>.Ok(result.Value!, "Login successful"));
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<AuthResponse>.Fail(result.Error!));

        return StatusCode(StatusCodes.Status201Created, ApiResponse<AuthResponse>.Ok(result.Value!, "Registration successful"));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<AuthResponse>.Fail(result.Error!));

        return Ok(ApiResponse<AuthResponse>.Ok(result.Value!, "Token refreshed"));
    }
}
