using Microsoft.AspNetCore.Identity.UI.Services;

namespace IPNoticeHub.Web.Infrastructure
{
    public sealed class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
            => Task.CompletedTask;
    }
}
