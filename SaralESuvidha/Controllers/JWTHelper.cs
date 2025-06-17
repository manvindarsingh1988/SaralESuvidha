using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using SaralESuvidha.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace SaralESuvidha.Controllers
{
    public class JWTHelper
    {
        public static (string, DateTime) GenerateJSONWebToken(UserInfo userInfo, IConfiguration config)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Name, userInfo.UserName),
                new Claim(JwtRegisteredClaimNames.Sid, userInfo.Id),
                new Claim(JwtRegisteredClaimNames.Typ, userInfo.UserType),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var exp = DateTime.Now.AddMinutes(120);
            var token = new JwtSecurityToken(config["Jwt:Issuer"],
                config["Jwt:Issuer"],
                claims,
                expires: exp,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), exp);
        }

        public static UserInfo GetCurrentUserDetails(string authHeader, IConfiguration config)
        {
            var accessToken = authHeader?.StartsWith("Bearer ") == true ? authHeader.Substring("Bearer ".Length) : null;

            // Validate and read claims from the expired token
            var handler = new JwtSecurityTokenHandler();
            var user = new UserInfo();

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidIssuer = config["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
                    ValidateLifetime = true, // Ignore expiry
                    ValidateIssuerSigningKey = true
                };

                var principal = handler.ValidateToken(accessToken, tokenValidationParameters, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extract user claims
                var userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value;
                var userName = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
                var userType = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Typ)?.Value;

                user = new UserInfo
                {
                    Id = userId,
                    UserName = userName,
                    UserType = userType
                };
            }
            catch { }

            return user;
        }
    }
}
