using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartPark.Application.Abstractions;
using SmartPark.Domain.IdentityAccess;

namespace SmartPark.Infrastructure.Identity;

/// <summary>Implementación de IJwtTokenService: emite JWT firmados con HMAC-SHA256.</summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _o = options.Value;

    public AuthToken Issue(UserAccount user)
    {
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.Key)), SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("name", user.FullName),
        };
        var expires = DateTimeOffset.UtcNow.AddMinutes(_o.ExpiresMinutes);
        var jwt = new JwtSecurityToken(_o.Issuer, _o.Audience, claims, expires: expires.UtcDateTime, signingCredentials: creds);
        return new AuthToken(new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
