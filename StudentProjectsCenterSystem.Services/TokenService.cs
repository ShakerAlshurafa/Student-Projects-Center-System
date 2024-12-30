using Castle.Core.Configuration;
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
        private readonly Microsoft.Extensions.Configuration.IConfiguration configuration;
        private readonly UserManager<LocalUser> userManager;
        private readonly string secretKey;

        public TokenService(
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            UserManager<LocalUser> userManager)
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
                new Claim(ClaimTypes.Name, localUser?.UserName ?? throw new InvalidOperationException("UserName is required")),
                new Claim(ClaimTypes.NameIdentifier, localUser?.Id ?? throw new InvalidOperationException("User ID is required")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
                new Claim(JwtRegisteredClaimNames.Sub, localUser?.Id ?? "") // Subject claim for user ID
            };

            // Fetch roles and add them as claims
            var roles = await userManager.GetRolesAsync(localUser ?? new LocalUser());
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtExpirationDays = configuration.GetValue<int>("TokenSettings:JWTExpirationDays");

            // Create the token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(jwtExpirationDays), // Token expiration time
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the generated token
            return tokenHandler.WriteToken(token);
        }
    }
}
