using IPNoticeHub.Data.Seed;

namespace IPNoticeHub.Web.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedIdentitiesAsync(this WebApplication app)
        {
            await IdentitySeeder.SeedIdentitiesAsync(app.Services);
        }

        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

            return app;
        }

        public static IEndpointRouteBuilder MapControllerRoutes(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return endpoints;
        }
    }
}
