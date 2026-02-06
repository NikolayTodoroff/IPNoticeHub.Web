using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories
{
    public class UserManagerMockFactory
    {
        public static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = 
                new Mock<IUserStore<ApplicationUser>>();

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
                null!,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
        }
    }
}
