using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TradingSystem.Access
{
    public class AuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] roles;

        public AuthorizationAttribute(string[] roles)
        {
            this.roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var currentRole = (context.HttpContext.Items.TryGetValue("role", out var role)
                ? role
                : string.Empty) as string;

            if (string.IsNullOrWhiteSpace(currentRole) ||
                !roles.Any(role => role.Equals(currentRole, StringComparison.InvariantCultureIgnoreCase)))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
