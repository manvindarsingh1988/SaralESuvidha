using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;
using System.Linq;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Extensions.DependencyInjection;

namespace SaralESuvidha
{
    public class Authentication
    {
        private IConfiguration _config;
        public Authentication(IConfiguration config)
        {
            _config = config;
        }

        public string ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Convert.ToString(_config["Jwt:Key"]));
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero

                }, out SecurityToken validatedToken);

                // Corrected access to the validatedToken
                var jwtToken = (JwtSecurityToken)validatedToken;
                var jku = jwtToken.Claims.First(claim => claim.Type == "jku").Value;
                var userName = jwtToken.Claims.First(claim => claim.Type == "kid").Value;

                return userName;
            }
            catch
            {
                return null;
            }
        }
    }

    public class JwtAuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var config = filterContext.HttpContext.RequestServices.GetService<IConfiguration>();
            var request = filterContext.HttpContext.Request;
            var token = request.Headers["Authorization"].ToString();

            if (token != null)
            {
                var auth = new Authentication(config);
                var userName = auth.ValidateToken(token);
                if (userName == null)
                {
                    filterContext.Result = new UnauthorizedResult();
                }
            }
            else
            {
                filterContext.Result = new UnauthorizedResult();
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
