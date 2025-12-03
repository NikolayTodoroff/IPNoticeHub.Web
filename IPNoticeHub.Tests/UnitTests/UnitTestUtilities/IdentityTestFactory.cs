using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.ApplicationUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPNoticeHub.Tests.UnitTests.UnitTestUtilities
{
    public class IdentityTestFactory
    {
        public static ServiceProvider BuildIdentityServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddDbContext<IPNoticeHubDbContext>(options =>
                options.UseInMemoryDatabase("IdentityTestDb"));

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

            return services.BuildServiceProvider();
        }
    }
}
