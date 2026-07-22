using System.Security.Claims;
using System.Text;
using identity.api.Configuration;
using identity.api.Models;
using identity.api.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace identity.unit.tests.Authentication;

public class TokenGeneratorTests
{
    [Fact(DisplayName = "Should issue a signed token with the configured identity claims")]
    public void Generate_ValidUser_ShouldIssueConfiguredToken()
    {
        const string signingKey = "unit-tests-only-signing-key-at-least-32-chars";
        var options = Options.Create(new JwtOptions
        {
            Issuer = "identity-tests",
            Audience = "identity-test-client",
            SigningKey = signingKey,
            ExpirationMinutes = 5
        });
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test.user",
            Role = new Role { RoleName = "admin" }
        };
        var generator = new TokenGenerator(options, TimeProvider.System);

        var token = generator.Generate(user);

        var principal = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = options.Value.Issuer,
                ValidAudience = options.Value.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ClockSkew = TimeSpan.Zero
            },
            out _);

        Assert.Equal(user.Username, principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Role.RoleName, principal.FindFirst(ClaimTypes.Role)?.Value);
    }
}
