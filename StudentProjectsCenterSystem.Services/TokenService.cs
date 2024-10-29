using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentProjectsCenterSystem.Services
{
    public class TokenService : ITokenServices
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<LocalUser> userManager;
        private readonly string secretKey;

        public TokenService(IConfiguration configuration, UserManager<LocalUser> userManager)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            secretKey = configuration.GetSection("ApiSetting")["SecretKey"]!;
        }

        public async Task<string> CreateTokenAsync(LocalUser localUser)
        {
            // Define the key using the secret from configuration
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            // Define the credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define the claims you want to add in the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, localUser?.Email ?? ""),
                new Claim(ClaimTypes.Name, localUser?.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await userManager.GetRolesAsync(localUser);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Create the token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token expiration time
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the generated token
            return tokenHandler.WriteToken(token);
        }
    }
}
