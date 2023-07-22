using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using identity.api.Models;
using Microsoft.IdentityModel.Tokens;

namespace identity.api.Utils;

public static class TokenGenerator
{
    public static string Generate(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = "token_65fb67e4-5f3b-4711-843d-07d4a5e61c72"u8.ToArray();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = "identity",
            Audience = "client",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}