using Microsoft.AspNetCore.Mvc;
using NtsHotels.Api.DTOs;
using NtsHotels.Api.Services;

namespace NtsHotels.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService users, ITokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
    {
        var user = await users.LoginAsync(dto);
        if (user is null) return Unauthorized("Credenciales inválidas.");

        var (token, expiresAt) = tokens.CreateToken(user);
        return Ok(new LoginResponseDto { Token = token, ExpiresAt = expiresAt, User = user });
    }
}
