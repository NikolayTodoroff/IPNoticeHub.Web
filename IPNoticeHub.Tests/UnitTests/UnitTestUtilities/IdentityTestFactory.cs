using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class IdentityTestHost : IDisposable
{
    private readonly SqliteConnection connection;
    public ServiceProvider Provider { get; }

    public IdentityTestHost()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<IPNoticeHubDbContext>(options =>
            options.UseSqlite(connection));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<IPNoticeHubDbContext>()
        .AddDefaultTokenProviders();

        Provider = services.BuildServiceProvider();

        using var scope = Provider.CreateScope();

        var testDbContext = 
            scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

        testDbContext.Database.EnsureCreated();
    }

    public IServiceScope CreateScope() => Provider.CreateScope();

    public void Dispose()
    {
        Provider.Dispose();
        connection.Dispose();
    }
}
