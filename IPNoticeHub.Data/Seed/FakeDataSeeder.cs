using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Seed
{
    public static class FakeDataSeeder
    {
        public static void Seed(ModelBuilder builder)
        {
            var user1Id = "2b195b12-9690-46b9-ac8e-50118a7102ea";
            var user2Id = "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b";
              
            // Seed Fake User Data
            builder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = user1Id,
                    UserName = "testuser1@example.com",
                    NormalizedUserName = "TESTUSER1@EXAMPLE.COM",
                    Email = "testuser1@example.com",
                    NormalizedEmail = "TESTUSER1@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFakeHash1234567890==",
                    SecurityStamp = "6fc62e55-dbc8-4022-84b4-cd49bdb663a3",
                    ConcurrencyStamp = "73dd29f5-cb9c-4f43-84ae-73597cded863"
                },
                new ApplicationUser
                {
                    Id = user2Id,
                    UserName = "testuser2@example.com",
                    NormalizedUserName = "TESTUSER2@EXAMPLE.COM",
                    Email = "testuser2@example.com",
                    NormalizedEmail = "TESTUSER2@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagFakeHash0987654321==",
                    SecurityStamp = "abc60a70-c408-4762-be78-c7c0d1ea3b9b",
                    ConcurrencyStamp = "d1d41029-3bbf-4476-890a-d64104220466"
                }
            );

            // Seed Fake Trademark Registrations
            builder.Entity<TrademarkRegistration>().HasData(
                new TrademarkRegistration
                {
                    Id = 1,
                    PublicId = Guid.Parse("4dd1f011-5362-42ff-9853-33ccbe4aa935"),
                    Wordmark = "Nike",
                    SourceId = "123456",
                    RegistrationNumber = "54321",
                    GoodsAndServices = "Clothing, footwear, headgear",
                    Owner = "Nike Inc.",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    FilingDate = new DateTime(2010, 1, 1),
                    RegistrationDate = new DateTime(2012, 1, 1),
                    Source = DataProvider.USPTO
                },
                new TrademarkRegistration
                {
                    Id = 2,
                    PublicId = Guid.Parse("fdb6b78f-6f5c-42ec-8b36-9a958011168b"),
                    Wordmark = "Adidas",
                    SourceId = "654321",
                    RegistrationNumber = "98765",
                    GoodsAndServices = "Sports equipment, clothing",
                    Owner = "Adidas AG",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    FilingDate = new DateTime(2015, 5, 5),
                    RegistrationDate = new DateTime(2017, 6, 6),
                    Source = DataProvider.USPTO
                }
            );

            builder.Entity<TrademarkClassAssignment>().HasData(
                new { TrademarkRegistrationId = 1, ClassNumber = 25 },
                new { TrademarkRegistrationId = 2, ClassNumber = 25 },
                new { TrademarkRegistrationId = 2, ClassNumber = 28 }
            );

            // Seed Fake Copyright Registrations
            builder.Entity<CopyrightRegistration>().HasData(
                new CopyrightRegistration
                {
                    Id = 1,
                    PublicId = Guid.Parse("076d6d16-235d-40e7-b419-da5465d8ebdf"),
                    RegistrationNumber = "VA0002288838",
                    TypeOfWork = "Visual Material",
                    Title = "Astronaut Music DJ",
                    YearOfCreation = 2020,
                    DateOfPublication = new DateTime(2020, 4, 16),
                    Owner = "Nikolay Todorov",
                    NationOfFirstPublication = "United States"
                }
            );

            // Relationships and Associations
            builder.Entity<UserTrademark>().HasData(
                new { ApplicationUserId = user1Id, TrademarkRegistrationId = 1, AddedToWatchlist = true, DateAdded = DateTime.UtcNow, IsDeleted = false },
                new { ApplicationUserId = user2Id, TrademarkRegistrationId = 2, AddedToWatchlist = false, DateAdded = DateTime.UtcNow, IsDeleted = false }
            );

            builder.Entity<UserCopyright>().HasData(
                new { ApplicationUserId = user1Id, CopyrightRegistrationId = 1, DateAdded = DateTime.UtcNow, IsDeleted = false }
            );
        }
    }
}

