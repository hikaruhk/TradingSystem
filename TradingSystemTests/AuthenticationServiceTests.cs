using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TradingSystem.Services;

namespace TradingSystemTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const int Expires = 1;

        [TestCase("UserABC", "passwordEFG", true, Description = "Is a valid user/pass")]
        public void ShouldGenerateTokenForValidUser(string username, string password, bool hasToken)
        {
            var configs = GetConfiguration();
            var service = new AuthenticationService(configs);
            var currentTimestamp = new DateTime(3020, 06, 04);
            
            var response = service.GetAuthenticationResponse(new AuthenticationRequest
            {
                Username = username,
                Password = password
            },
            currentTimestamp);

            response
                .Should()
                .NotBeNull()
                .And
                .BeEquivalentTo(new AuthenticationResponse
                {
                    Username = username,
                    Expires = currentTimestamp.AddMinutes(Expires),
                },
                options => options.Excluding(e => e.Token));

            response
                .Token
                .Should()
                .NotBeNullOrEmpty();
            
        }

        [TestCase("UserABC", "", Description = "Is not a valid user")]
        [TestCase("", "pass1", Description = "Is not a valid user")]
        [TestCase("", "", Description = "Is not a valid user")]
        public void ShouldGenerateTokenForInvalidUser(string username, string password)
        {
            var configs = GetConfiguration();
            var service = new AuthenticationService(configs);
            var currentTimestamp = new DateTime(3020, 06, 04);

            var response = service.GetAuthenticationResponse(new AuthenticationRequest
            {
                Username = username,
                Password = password
            },
            currentTimestamp);

            response
                .Should()
                .NotBeNull()
                .And
                .BeEquivalentTo(new AuthenticationResponse
                {
                    Username = username,
                    Expires = DateTime.MinValue,
                },
                options => options.Excluding(e => e.Token));

            response
                .Token
                .Should()
                .BeEmpty();
        }

        private IConfiguration GetConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "auth:secret", "thisissomesecret" },
                { "auth:expire", Expires.ToString() },
                { "auth:users:UserABC", "passwordEFG" },
                { "auth:users:UserTrader", "pass1" },
                { "auth:users:UserAudit", "pass2" }
            });

            return configBuilder.Build();
        }
    }
}