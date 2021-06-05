using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TradingSystem.Access;

namespace TradingSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticateController : ControllerBase
    {
        private readonly IAuthenticationService authService;

        public AuthenticateController(IAuthenticationService authService)
        {
            this.authService = authService;
        }

        [HttpPost]
        public IActionResult GetToken([Required][FromBody]AuthenticationRequest request)
        {
            var token = authService.GetAuthenticationResponse(request, DateTime.UtcNow);
            
            return Ok(token);
        }
    }
}
