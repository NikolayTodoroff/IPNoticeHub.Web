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
        public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TimeProvider timeProvider)      
        : base(options, logger, encoder, timeProvider) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Resolve factory from DI to know which user we want
            var factory = Context.RequestServices.GetService<TestWebAppFactoryAccessor>();
            var userId = factory?.Factory?.GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return Task.FromResult(AuthenticateResult.NoResult());

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
