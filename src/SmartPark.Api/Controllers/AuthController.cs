using Microsoft.AspNetCore.Mvc;
using SmartPark.Application.IdentityAccess;
using SmartPark.Domain.Common;

namespace SmartPark.Api.Controllers;

/// <summary>Endpoints de registro y autenticación (TS-09). Bounded Context: Identity &amp; Access.</summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(RegisterUserHandler register, LoginHandler login) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterUserCommand command, CancellationToken ct)
    {
        try
        {
            var result = await register.HandleAsync(command, ct);
            return Created("/api/v1/auth/login", result);
        }
        catch (DomainException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginQuery query, CancellationToken ct)
    {
        var result = await login.HandleAsync(query, ct);
        return result is null ? Unauthorized(new { message = "Credenciales inválidas." }) : Ok(result);
    }
}
