using System;

namespace TradingSystem.Services
{
    public interface IAuthenticationService
    {
        AuthenticationResponse GetAuthenticationResponse(AuthenticationRequest request, DateTime expires);
    }
}