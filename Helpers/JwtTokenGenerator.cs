// FILE: Helpers/JwtTokenGenerator.cs
// PURPOSE: Builds the signed JWT returned at login
//          (04-BACKEND-ARCHITECTURE.md section 2's documented Helpers/ file).
//          Used only by AuthController.
// LOGIC:   - The role claim uses ClaimTypes.Role (the .NET default), which
//            is what ASP.NET Core's own [Authorize(Roles = "Admin")]
//            attribute reads automatically - no extra configuration needed
//            for server-side authorization to work.
//          - "exp" is written by JwtSecurityToken's own `expires` parameter,
//            which the underlying library always serializes under the
//            short, standard JWT claim name "exp" - this is the one claim
//            the frontend's own token decoder actually reads
//            (shared/js/auth.js isAuthenticated() checks `payload.exp`), so
//            this matters and is verified to work independently of how the
//            role claim itself is named internally.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ScanEat.Api.Models.Entities;

namespace ScanEat.Api.Helpers;

public class JwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Builds a signed JWT carrying the user's name and role(s).
    public string GenerateToken(AppUser user, IList<string> roles)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var keyString = jwtSection["Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured in appsettings.json.");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiryHours = jwtSection.GetValue("ExpiryHours", 8);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserName ?? user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
