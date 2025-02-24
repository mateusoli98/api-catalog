using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalog.Services
{
    public interface ITokenService
    {
        JwtSecurityToken CreateToken(IEnumerable<Claim> claims, IConfiguration _congfig);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _congfig);
    }
}
