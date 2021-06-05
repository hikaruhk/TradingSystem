using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TradingSystem.Access;

namespace TradingSystemTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const int Expires = 1;

        [TestCase("UserABC", "passwordEFG", Description = "Is a valid user/pass")]
        public void ShouldGenerateTokenForValidUser(string username, string password)
        {
            var configs = GetConfiguration();
            var service = new AuthenticationService(configs);
            var currentTimestamp = new DateTime(2022, 06, 04);
            
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

        [TestCase(AuditValidToken, ExpectedResult = "audit", Description = "Audit user should be audit role")]
        [TestCase(TraderValidToken, ExpectedResult = "trader", Description = "Trader user should be audit trader")]
        public string ShouldRetrieveRoleFromToken(string token)
        {
            var configs = GetConfiguration();
            var service = new AuthenticationService(configs);

            var response = service.GetRole(token);

            return response;
        }

        [TestCase(InvalidToken, Description = "InvalidToken")]
        public void ShouldNotRetrieveRoleFromToken(string token)
        {
            var configs = GetConfiguration();
            var service = new AuthenticationService(configs);

            var response = service.GetRole(token);

            response.Should().BeNullOrWhiteSpace();
        }

        private const string AuditValidToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiVXNlckFCQyIsInJvbGUiOiJhdWRpdCIsIm5iZiI6MTYyMjg1NjE1MiwiZXhwIjoxNjU0MzI2MDYwLCJpYXQiOjE2MjI4NTYxNTJ9.llOCsun_SOmYZf00E3_Qkklgel6DlqlnYML0LY1Lg_4";
        private const string TraderValidToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiVXNlclRyYWRlciIsInJvbGUiOiJ0cmFkZXIiLCJuYmYiOjE2MjI4NTYxMDIsImV4cCI6MTY1NDMyNjA2MCwiaWF0IjoxNjIyODU2MTAyfQ.n_iTC4W1fAVe9IBiYT98giEceZQj5nAMZJ-x7wtOKag";
        private const string InvalidToken = "ABC.n_iTC4W1fAVe9IBiYT98giEceZQj5nAMZJ-x7wtOKag";

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