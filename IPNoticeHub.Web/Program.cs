using IPNoticeHub.Web.Extensions;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace IPNoticeHub.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddIdentityModule();
            builder.Services.AddUserRegistration();
            builder.Services.AddAuthorizationPolicies();
            builder.Services.AddCookieConfiguration();
            builder.Services.AddPresentationLayer();

            builder.Services.AddTrademarkModule();
            builder.Services.AddCopyrightModule();
            builder.Services.AddDocumentLibraryModule();
            builder.Services.AddPdfGeneration();
            builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();

            var app = builder.Build();

            await app.SeedIdentitiesAsync();

            app.UseExceptionHandling(app.Environment);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoutes();
            app.MapRazorPages();

            app.Run();
        }
    }
}
