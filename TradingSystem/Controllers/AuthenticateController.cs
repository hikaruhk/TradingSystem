using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetToken(
            [Required][FromHeader]string username,
            [Required][FromHeader]string password)
        {
            var token = authService.GetAuthenticationResponse(
                new AuthenticationRequest { Username = username, Password = password },
                DateTime.UtcNow);
            
            return Ok(token);
        }
    }
}
