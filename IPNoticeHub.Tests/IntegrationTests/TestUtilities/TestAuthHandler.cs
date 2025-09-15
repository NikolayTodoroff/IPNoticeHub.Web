using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace IPNoticeHub.Tests.IntegrationTests.TestUtilities
{
    public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
        {
            var currentUtc = TimeProvider.GetUtcNow();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Retrieve the TestWebAppFactory instance from the dependency injection container 
            // to determine the current user ID for authentication.
            var testAppFactory = Context.RequestServices.GetService<TestWebAppFactoryAccessor>();
            var userId = testAppFactory?.Factory?.GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }            

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, authenticationScheme: "TestAuth");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    // A tiny accessor so handler can reach the factory instance
    public sealed class TestWebAppFactoryAccessor
    {
        public TestWebAppFactory? Factory { get; init; }
    }
}
