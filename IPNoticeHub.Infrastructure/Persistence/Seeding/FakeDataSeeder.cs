using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class FakeDataSeeder
    {
        public static void Seed(ModelBuilder builder)
        {
            var user1Id = "2b195b12-9690-46b9-ac8e-50118a7102ea";
            var user2Id = "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b";

            var seedDateUtc = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

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

            builder.Entity<TrademarkEntity>().HasData(
                new TrademarkEntity
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
                new TrademarkEntity
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
                },

                new TrademarkEntity
                {
                    Id = 3,
                    PublicId = Guid.Parse("7a6b6f2b-3e1d-4c44-9b4b-9aef4e4f3a10"),
                    Wordmark = "IPNOTICEHUB",
                    SourceId = "IPN-0001",
                    RegistrationNumber = "11111",
                    GoodsAndServices = "Software as a service (SaaS) for IP monitoring and document generation",
                    Owner = "IPNoticeHub Ltd.",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    FilingDate = new DateTime(2023, 2, 1),
                    RegistrationDate = new DateTime(2024, 1, 15),
                    Source = DataProvider.USPTO
                },
                new TrademarkEntity
                {
                    Id = 4,
                    PublicId = Guid.Parse("2bdb1d8a-1a11-4b3a-92f7-2c56e1c6f0c2"),
                    Wordmark = "CLOUDARMOR",
                    SourceId = "CA-2040",
                    RegistrationNumber = "22222",
                    GoodsAndServices = "Cloud security consulting and DevOps deployment services",
                    Owner = "Cloud Armor Group",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    FilingDate = new DateTime(2022, 10, 10),
                    RegistrationDate = new DateTime(2023, 8, 20),
                    Source = DataProvider.USPTO
                }
            );

            builder.Entity<TrademarkClassAssignment>().HasData(
                new { TrademarkRegistrationId = 1, ClassNumber = 25 },
                new { TrademarkRegistrationId = 2, ClassNumber = 25 },
                new { TrademarkRegistrationId = 2, ClassNumber = 28 },

                new { TrademarkRegistrationId = 3, ClassNumber = 9 },
                new { TrademarkRegistrationId = 3, ClassNumber = 42 },
                new { TrademarkRegistrationId = 4, ClassNumber = 35 },
                new { TrademarkRegistrationId = 4, ClassNumber = 42 }
            );

            builder.Entity<CopyrightEntity>().HasData(
                new CopyrightEntity
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
                },

                new CopyrightEntity
                {
                    Id = 2,
                    PublicId = Guid.Parse("c1d0c5bf-0b4b-4c9a-9cc0-2a3c9b2b0f11"),
                    RegistrationNumber = "TX0009990001",
                    TypeOfWork = "Literary Work",
                    Title = "IPNoticeHub – Terms & Templates Collection",
                    YearOfCreation = 2024,
                    DateOfPublication = new DateTime(2024, 6, 1),
                    Owner = "IPNoticeHub Ltd.",
                    NationOfFirstPublication = "United States"
                },
                new CopyrightEntity
                {
                    Id = 3,
                    PublicId = Guid.Parse("b7a97c4a-6ee3-4b2f-a7b2-5b3a1d1a7c77"),
                    RegistrationNumber = "TX0009990002",
                    TypeOfWork = "Computer Program",
                    Title = "IPNoticeHub – Core Services",
                    YearOfCreation = 2024,
                    DateOfPublication = new DateTime(2024, 9, 10),
                    Owner = "IPNoticeHub Ltd.",
                    NationOfFirstPublication = "United States"
                }
            );

            builder.Entity<UserTrademark>().HasData(
                new
                {
                    ApplicationUserId = user1Id,
                    TrademarkEntityId = 1,
                    DateAdded = seedDateUtc,
                    IsDeleted = false,
                    WatchlistNotificationsEnabled = false
                },
                new
                {
                    ApplicationUserId = user2Id,
                    TrademarkEntityId = 2,
                    DateAdded = seedDateUtc,
                    IsDeleted = false,
                    WatchlistNotificationsEnabled = false
                },

                new
                {
                    ApplicationUserId = user1Id,
                    TrademarkEntityId = 3,
                    DateAdded = seedDateUtc,
                    IsDeleted = false,
                    WatchlistNotificationsEnabled = true
                },
                new
                {
                    ApplicationUserId = user2Id,
                    TrademarkEntityId = 4,
                    DateAdded = seedDateUtc,
                    IsDeleted = false,
                    WatchlistNotificationsEnabled = true
                }
            );

            builder.Entity<UserCopyright>().HasData(
                new
                {
                    ApplicationUserId = user1Id,
                    CopyrightEntityId = 1,
                    DateAdded = seedDateUtc,
                    IsDeleted = false
                },
                new
                {
                    ApplicationUserId = user1Id,
                    CopyrightEntityId = 2,
                    DateAdded = seedDateUtc,
                    IsDeleted = false
                },
                new
                {
                    ApplicationUserId = user2Id,
                    CopyrightEntityId = 3,
                    DateAdded = seedDateUtc,
                    IsDeleted = false
                }
            );
        }
    }
}
