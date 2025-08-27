using System;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Ecommerce.Infrastructure.UnitTests.Auth
{
    public class AuthServiceTests
    {
        private static IConfiguration BuildConfig(params (string key, string? value)[] entries)
        {
            var dict = new Dictionary<string, string?>();
            foreach (var (key, value) in entries) dict[key] = value;
            return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        [Fact]
        public void GenerateJwtToken_ShouldThrow_WhenKeyMissing()
        {
            var config = BuildConfig(("Jwt:Issuer", "issuer"), ("Jwt:Audience", "aud"));
            var svc = new AuthService(config);
            var user = new User { Id = Guid.NewGuid(), Email = "u@test.com", Role = "Customer" };
            Assert.Throws<InvalidOperationException>(() => svc.GenerateJwtToken(user));
        }

        [Fact]
        public void GenerateJwtToken_ShouldUseEnvFallback_WhenConfigMissing()
        {
            var previousKey = Environment.GetEnvironmentVariable("JWT_KEY");
            var previousIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var previousAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            try
            {
                Environment.SetEnvironmentVariable("JWT_KEY", "UnitTest_SuperSecretKey_AtLeast_32_Chars_Length_");
                Environment.SetEnvironmentVariable("JWT_ISSUER", "IssuerFromEnv");
                Environment.SetEnvironmentVariable("JWT_AUDIENCE", "AudienceFromEnv");

                var config = BuildConfig();
                var svc = new AuthService(config);
                var user = new User { Id = Guid.NewGuid(), Email = "u@test.com", Role = "Customer" };
                var token = svc.GenerateJwtToken(user);
                Assert.False(string.IsNullOrWhiteSpace(token));
            }
            finally
            {
                Environment.SetEnvironmentVariable("JWT_KEY", previousKey);
                Environment.SetEnvironmentVariable("JWT_ISSUER", previousIssuer);
                Environment.SetEnvironmentVariable("JWT_AUDIENCE", previousAudience);
            }
        }
    }
}
