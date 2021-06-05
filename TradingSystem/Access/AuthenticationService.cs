using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TradingSystem.Access
{
    /// <summary>
    /// Provides the mechanism to reach out and get a JWT token
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration config;

        public AuthenticationService(IConfiguration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Given an authentication request, returns an authentication response, even if the user
        /// is not found
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AuthenticationResponse GetAuthenticationResponse(
            AuthenticationRequest request,
            DateTime expires)
        {
            //This represents reaching out to an external service for authentication.
            var users = config.GetSection("auth:users").Get<Dictionary<string, string>>();
            var exists = users.TryGetValue(request.Username, out var pass) 
                && pass == request.Password;

            var expiresIn = config.GetSection("auth:expire").Get<int>();
            var expire = expires.AddMinutes(expiresIn);

            return !exists
                ? new AuthenticationResponse { Username = request.Username, Expires = DateTime.MinValue, Token = string.Empty }
                : new AuthenticationResponse
                {
                    Username = request.Username,
                    Token = GetJwtToken(request, expire),
                    Expires = expire
                };
        }

        /// <summary>
        /// Takes a token and extracts the role from the given user, but only if it is not expired.
        /// If expired, or has no role, returns empty string.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string GetRole(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var secret = config.GetSection("auth:secret").Get<string>();
            var encodedSecret = Encoding.UTF8.GetBytes(secret);

            handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(encodedSecret),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false
                },
                out var securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            return jwtSecurityToken.ValidTo < DateTime.UtcNow
                ? string.Empty
                : jwtSecurityToken
                    .Claims
                    .SingleOrDefault(s => s.Type == "role")
                    ?.Value ?? string.Empty;
        }

        private string GetJwtToken(AuthenticationRequest request, DateTime expires)
        {
            var handler = new JwtSecurityTokenHandler();
            var secret = config.GetSection("auth:secret").Get<string>();
            var encodedSecret = Encoding.UTF8.GetBytes(secret);

            //This would realistically not be done here.
            var role = request.Username.Contains("Trader")
                ? new Claim("role", "trader")
                : new Claim("role", "audit");

            var token = handler.CreateJwtSecurityToken(
                new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("name", request.Username), role }),
                    Expires = expires,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(encodedSecret),
                        SecurityAlgorithms.HmacSha256Signature)
                });

            var serializedToken = handler.WriteToken(token);
            return serializedToken;
        }
    }
}
