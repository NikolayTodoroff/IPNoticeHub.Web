using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace IPNoticeHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedFakeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "2b195b12-9690-46b9-ac8e-50118a7102ea", 0, "73dd29f5-cb9c-4f43-84ae-73597cded863", "testuser1@example.com", true, false, null, "TESTUSER1@EXAMPLE.COM", "TESTUSER1@EXAMPLE.COM", "AQAAAAIAAYagAAAAEFakeHash1234567890==", null, false, "6fc62e55-dbc8-4022-84b4-cd49bdb663a3", false, "testuser1@example.com" },
                    { "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b", 0, "d1d41029-3bbf-4476-890a-d64104220466", "testuser2@example.com", true, false, null, "TESTUSER2@EXAMPLE.COM", "TESTUSER2@EXAMPLE.COM", "AQAAAAIAAYagFakeHash0987654321==", null, false, "abc60a70-c408-4762-be78-c7c0d1ea3b9b", false, "testuser2@example.com" }
                });

            migrationBuilder.InsertData(
                table: "CopyrightRegistrations",
                columns: new[] { "Id", "DateOfPublication", "NationOfFirstPublication", "Owner", "PublicId", "RegistrationNumber", "Title", "TypeOfWork", "YearOfCreation" },
                values: new object[] { 1, new DateTime(2020, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "United States", "Nikolay Todorov", new Guid("076d6d16-235d-40e7-b419-da5465d8ebdf"), "VA0002288838", "Astronaut Music DJ", "Visual Material", 2020 });

            migrationBuilder.InsertData(
                table: "TrademarkRegistrations",
                columns: new[] { "Id", "FilingDate", "GoodsAndServices", "MarkImageUrl", "Owner", "PublicId", "RegistrationDate", "RegistrationNumber", "Source", "SourceId", "StatusCategory", "StatusDetail", "Wordmark" },
                values: new object[,]
                {
                    { 1, new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Clothing, footwear, headgear", null, "Nike Inc.", new Guid("4dd1f011-5362-42ff-9853-33ccbe4aa935"), new DateTime(2012, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "54321", 0, "123456", 1, "Live/Registered", "Nike" },
                    { 2, new DateTime(2015, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sports equipment, clothing", null, "Adidas AG", new Guid("fdb6b78f-6f5c-42ec-8b36-9a958011168b"), new DateTime(2017, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "98765", 0, "654321", 1, "Live/Registered", "Adidas" }
                });

            migrationBuilder.InsertData(
                table: "TrademarkClassAssignment",
                columns: new[] { "ClassNumber", "TrademarkRegistrationId" },
                values: new object[,]
                {
                    { 25, 1 },
                    { 25, 2 },
                    { 28, 2 }
                });

            migrationBuilder.InsertData(
                table: "UserCopyright",
                columns: new[] { "ApplicationUserId", "CopyrightRegistrationId", "DateAdded", "IsDeleted" },
                values: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1, new DateTime(2025, 8, 25, 18, 57, 46, 431, DateTimeKind.Utc).AddTicks(7182), false });

            migrationBuilder.InsertData(
                table: "UserTrademark",
                columns: new[] { "ApplicationUserId", "TrademarkRegistrationId", "AddedToWatchlist", "DateAdded", "IsDeleted" },
                values: new object[,]
                {
                    { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1, true, new DateTime(2025, 8, 25, 18, 57, 46, 431, DateTimeKind.Utc).AddTicks(7161), false },
                    { "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b", 2, false, new DateTime(2025, 8, 25, 18, 57, 46, 431, DateTimeKind.Utc).AddTicks(7164), false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TrademarkClassAssignment",
                keyColumns: new[] { "ClassNumber", "TrademarkRegistrationId" },
                keyValues: new object[] { 25, 1 });

            migrationBuilder.DeleteData(
                table: "TrademarkClassAssignment",
                keyColumns: new[] { "ClassNumber", "TrademarkRegistrationId" },
                keyValues: new object[] { 25, 2 });

            migrationBuilder.DeleteData(
                table: "TrademarkClassAssignment",
                keyColumns: new[] { "ClassNumber", "TrademarkRegistrationId" },
                keyValues: new object[] { 28, 2 });

            migrationBuilder.DeleteData(
                table: "UserCopyright",
                keyColumns: new[] { "ApplicationUserId", "CopyrightRegistrationId" },
                keyValues: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1 });

            migrationBuilder.DeleteData(
                table: "UserTrademark",
                keyColumns: new[] { "ApplicationUserId", "TrademarkRegistrationId" },
                keyValues: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1 });

            migrationBuilder.DeleteData(
                table: "UserTrademark",
                keyColumns: new[] { "ApplicationUserId", "TrademarkRegistrationId" },
                keyValues: new object[] { "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b", 2 });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2b195b12-9690-46b9-ac8e-50118a7102ea");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b");

            migrationBuilder.DeleteData(
                table: "CopyrightRegistrations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TrademarkRegistrations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TrademarkRegistrations",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
