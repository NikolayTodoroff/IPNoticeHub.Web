using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPNoticeHub.Tests.IntegrationTests.IntegrationTestUtilities
{
    public class UserManagerMockFactory
    {
        public static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            var options =
                Microsoft.Extensions.Options.Options.Create(new IdentityOptions());

            var pwdHasher =
                new Mock<IPasswordHasher<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                options,
                pwdHasher.Object,
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
        }
    }
}
