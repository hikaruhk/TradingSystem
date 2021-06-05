using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TradingSystem.Access
{
    public class AuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string role;

        public AuthorizationAttribute(string role)
        {
            this.role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var currentRole = context.HttpContext.Items["role"].ToString();

            if (string.IsNullOrWhiteSpace(currentRole) ||
                !role.Equals(currentRole, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
