using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories
{
    public sealed class NoOpAntiforgery : IAntiforgery
    {
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
            => new(
                "__cookie__", 
                "__form__", 
                "__header__", 
                "__requestToken__");

        public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
            => new(
                "__cookie__", 
                "__form__", 
                "__header__", 
                "__requestToken__");

        public Task<bool> IsRequestValidAsync(HttpContext httpContext) => 
            Task.FromResult(true);

        public Task ValidateRequestAsync(HttpContext httpContext) => 
            Task.CompletedTask;
        
        public string? CookieName => "__NoOp_Antiforgery";
        public string? HeaderName => "__NoOp_AF_Header";
        public string? FormFieldName => "__RequestVerificationToken";

        public void SetCookieTokenAndHeader(HttpContext httpContext) {}
    }
}
