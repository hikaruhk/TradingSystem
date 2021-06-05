using Microsoft.AspNetCore.Mvc.Filters;

namespace TradingSystem.Access
{
    public class AuthorizationMiddleware : IAuthorizationFilter
    {
        private readonly IAuthenticationService authenticationService;

        public AuthorizationMiddleware(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var role = authenticationService.GetRole(token);
                context.HttpContext.Items["role"] = role;
            }
        }
    }
}
