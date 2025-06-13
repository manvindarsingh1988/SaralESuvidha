using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using SaralESuvidha.Models;
using Microsoft.Extensions.Configuration;

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
    }
}
