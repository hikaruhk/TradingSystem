using System;

namespace TradingSystem.Services
{
    public class AuthenticationResponse
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
