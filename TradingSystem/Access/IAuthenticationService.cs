using System;

namespace TradingSystem.Access
{
    public interface IAuthenticationService
    {
        AuthenticationResponse GetAuthenticationResponse(AuthenticationRequest request, DateTime expires);
        string GetRole(string token);
    }
}