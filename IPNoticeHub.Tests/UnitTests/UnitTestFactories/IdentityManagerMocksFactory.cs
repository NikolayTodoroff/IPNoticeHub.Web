using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.Extensions.Logging;

namespace IPNoticeHub.Tests.UnitTests.UnitTestFactories
{
    public class IdentityManagerMocksFactory
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
                null,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
        }

        public static Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = 
                new Mock<IRoleStore<IdentityRole>>();

            var roleValidators = new IRoleValidator<IdentityRole>[]
            {
                new RoleValidator<IdentityRole>()
            };

            return new Mock<RoleManager<IdentityRole>>(
                store.Object,
                roleValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object);
        }
    }
}
