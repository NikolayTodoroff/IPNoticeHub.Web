using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Repositories.Application.Abstractions;
using IPNoticeHub.Data.Repositories.Application.Implementations;
using IPNoticeHub.Data.Repositories.Copyrights.Abstractions;
using IPNoticeHub.Data.Repositories.Copyrights.Implementations;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.Extensions;
using IPNoticeHub.Services.Application.Implementations;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.Implementations;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.Implementations;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QuestPDF.Infrastructure;
using IPNoticeHub.Data.Seed;
using static IPNoticeHub.Common.ValidationConstants.AuthRedirectPaths;
using Microsoft.AspNetCore.Identity;

namespace IPNoticeHub.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<IPNoticeHubDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<IPNoticeHubDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = LoginPath;
                options.AccessDeniedPath = AccessDeniedPath;
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("HasUserId", p => p.RequireClaim(ClaimTypes.NameIdentifier));
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<ITrademarkRepository, TrademarkRepository>();
            builder.Services.AddScoped<IUserTrademarkRepository, UserTrademarkRepository>();
            builder.Services.AddScoped<ITrademarkSearchService, TrademarkSearchService>();
            builder.Services.AddScoped<ITrademarkCollectionService, TrademarkCollectionService>();
            builder.Services.AddScoped<ICopyrightRepository, CopyrightRepository>();
            builder.Services.AddScoped<IUserCopyrightRepository, UserCopyrightRepository>();
            builder.Services.AddScoped<ICopyrightService, CopyrightService>();
            builder.Services.AddScoped<ITrademarkReadRepository, TrademarkReadRepository>();
            builder.Services.AddScoped<ITrademarkSearchQueryService, TrademarkSearchQueryService>();
            builder.Services.AddScoped<ITrademarkStatusSnapshotRepository, TrademarkStatusSnapshotRepository>();
            builder.Services.AddScoped<IUserTrademarkWatchlistRepository, UserTrademarkWatchlistRepository>();
            builder.Services.AddScoped<ITrademarkWatchlistService, TrademarkWatchlistService>();
            builder.Services.AddSingleton<IStatusLabelProvider, StatusLabelProvider>();
            builder.Services.AddScoped<IPdfService, PdfService>();
            builder.Services.AddSingleton<ILetterTemplateProvider, LetterTemplateProvider>();
            builder.Services.AddRazorPages();

            QuestPDF.Settings.License = LicenseType.Community;

            // Temporary no-op email sender while theming
            builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();

            var app = builder.Build();

            await IdentitySeeder.SeedIdentitiesAsync(app.Services);

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
