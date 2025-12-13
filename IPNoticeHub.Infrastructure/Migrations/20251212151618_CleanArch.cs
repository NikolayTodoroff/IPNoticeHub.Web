using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IPNoticeHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanArch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CopyrightRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TypeOfWork = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    YearOfCreation = table.Column<int>(type: "int", nullable: true),
                    DateOfPublication = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NationOfFirstPublication = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CopyrightRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Wordmark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GoodsAndServices = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StatusCategory = table.Column<int>(type: "int", nullable: false),
                    StatusDetail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StatusCodeRaw = table.Column<int>(type: "int", nullable: true),
                    StatusDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FilingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MarkImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegalDocuments",
                columns: table => new
                {
                    LegalDocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelatedPublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    TemplateType = table.Column<int>(type: "int", nullable: false),
                    DocumentTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IpTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SenderAddress = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    SenderEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RecipientAddress = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LetterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InfringingUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    GoodFaithStatement = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    AdditionalFacts = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    YearOfCreation = table.Column<int>(type: "int", nullable: true),
                    DateOfPublication = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NationOfFirstPublication = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocuments", x => x.LegalDocumentId);
                    table.ForeignKey(
                        name: "FK_LegalDocuments_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserCopyrights",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CopyrightEntityId = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCopyrights", x => new { x.ApplicationUserId, x.CopyrightEntityId });
                    table.ForeignKey(
                        name: "FK_UserCopyrights_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCopyrights_CopyrightRegistrations_CopyrightEntityId",
                        column: x => x.CopyrightEntityId,
                        principalTable: "CopyrightRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkClassAssignment",
                columns: table => new
                {
                    ClassNumber = table.Column<int>(type: "int", nullable: false),
                    TrademarkRegistrationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkClassAssignment", x => new { x.TrademarkRegistrationId, x.ClassNumber });
                    table.ForeignKey(
                        name: "FK_TrademarkClassAssignment_TrademarkRegistrations_TrademarkRegistrationId",
                        column: x => x.TrademarkRegistrationId,
                        principalTable: "TrademarkRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrademarkId = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventTypeRaw = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrademarkEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrademarkEvents_TrademarkRegistrations_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "TrademarkRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTrademarks",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrademarkEntityId = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrademarks", x => new { x.ApplicationUserId, x.TrademarkEntityId });
                    table.ForeignKey(
                        name: "FK_UserTrademarks_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTrademarks_TrademarkRegistrations_TrademarkEntityId",
                        column: x => x.TrademarkEntityId,
                        principalTable: "TrademarkRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Watchlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrademarkId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AddedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitialStatusCodeRaw = table.Column<int>(type: "int", nullable: true),
                    InitialStatusText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InitialStatusDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Watchlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Watchlists_TrademarkRegistrations_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "TrademarkRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                values: new object[,]
                {
                    { 1, new DateTime(2020, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "United States", "Nikolay Todorov", new Guid("076d6d16-235d-40e7-b419-da5465d8ebdf"), "VA0002288838", "Astronaut Music DJ", "Visual Material", 2020 },
                    { 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "United States", "IPNoticeHub Ltd.", new Guid("c1d0c5bf-0b4b-4c9a-9cc0-2a3c9b2b0f11"), "TX0009990001", "IPNoticeHub – Terms & Templates Collection", "Literary Work", 2024 },
                    { 3, new DateTime(2024, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "United States", "IPNoticeHub Ltd.", new Guid("b7a97c4a-6ee3-4b2f-a7b2-5b3a1d1a7c77"), "TX0009990002", "IPNoticeHub – Core Services", "Computer Program", 2024 }
                });

            migrationBuilder.InsertData(
                table: "TrademarkRegistrations",
                columns: new[] { "Id", "FilingDate", "GoodsAndServices", "MarkImageUrl", "Owner", "PublicId", "RegistrationDate", "RegistrationNumber", "Source", "SourceId", "StatusCategory", "StatusCodeRaw", "StatusDateUtc", "StatusDetail", "Wordmark" },
                values: new object[,]
                {
                    { 1, new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Clothing, footwear, headgear", null, "Nike Inc.", new Guid("4dd1f011-5362-42ff-9853-33ccbe4aa935"), new DateTime(2012, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "54321", 0, "123456", 1, null, null, "Live/Registered", "Nike" },
                    { 2, new DateTime(2015, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sports equipment, clothing", null, "Adidas AG", new Guid("fdb6b78f-6f5c-42ec-8b36-9a958011168b"), new DateTime(2017, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "98765", 0, "654321", 1, null, null, "Live/Registered", "Adidas" },
                    { 3, new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Software as a service (SaaS) for IP monitoring and document generation", null, "IPNoticeHub Ltd.", new Guid("7a6b6f2b-3e1d-4c44-9b4b-9aef4e4f3a10"), new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "11111", 0, "IPN-0001", 1, null, null, "Live/Registered", "IPNOTICEHUB" },
                    { 4, new DateTime(2022, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cloud security consulting and DevOps deployment services", null, "Cloud Armor Group", new Guid("2bdb1d8a-1a11-4b3a-92f7-2c56e1c6f0c2"), new DateTime(2023, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "22222", 0, "CA-2040", 1, null, null, "Live/Registered", "CLOUDARMOR" }
                });

            migrationBuilder.InsertData(
                table: "TrademarkClassAssignment",
                columns: new[] { "ClassNumber", "TrademarkRegistrationId" },
                values: new object[,]
                {
                    { 25, 1 },
                    { 25, 2 },
                    { 28, 2 },
                    { 9, 3 },
                    { 42, 3 },
                    { 35, 4 },
                    { 42, 4 }
                });

            migrationBuilder.InsertData(
                table: "UserCopyrights",
                columns: new[] { "ApplicationUserId", "CopyrightEntityId", "DateAdded", "IsDeleted" },
                values: new object[,]
                {
                    { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false },
                    { "2b195b12-9690-46b9-ac8e-50118a7102ea", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false },
                    { "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false }
                });

            migrationBuilder.InsertData(
                table: "UserTrademarks",
                columns: new[] { "ApplicationUserId", "TrademarkEntityId", "DateAdded", "IsDeleted" },
                values: new object[,]
                {
                    { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false },
                    { "2b195b12-9690-46b9-ac8e-50118a7102ea", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false },
                    { "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false },
                    { "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b", 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CopyrightRegistrations_RegistrationNumber",
                table: "CopyrightRegistrations",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_ApplicationUserId",
                table: "LegalDocuments",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkEvents_TrademarkId",
                table: "TrademarkEvents",
                column: "TrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkRegistrations_RegistrationNumber",
                table: "TrademarkRegistrations",
                column: "RegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkRegistrations_SourceId",
                table: "TrademarkRegistrations",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "UX_Trademark_Source_SourceId",
                table: "TrademarkRegistrations",
                columns: new[] { "Source", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCopyrights_ApplicationUserId_CopyrightEntityId",
                table: "UserCopyrights",
                columns: new[] { "ApplicationUserId", "CopyrightEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCopyrights_CopyrightEntityId",
                table: "UserCopyrights",
                column: "CopyrightEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrademarks_TrademarkEntityId",
                table: "UserTrademarks",
                column: "TrademarkEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Watchlists_TrademarkId",
                table: "Watchlists",
                column: "TrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Watchlists_UserId_TrademarkId",
                table: "Watchlists",
                columns: new[] { "UserId", "TrademarkId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "LegalDocuments");

            migrationBuilder.DropTable(
                name: "TrademarkClassAssignment");

            migrationBuilder.DropTable(
                name: "TrademarkEvents");

            migrationBuilder.DropTable(
                name: "UserCopyrights");

            migrationBuilder.DropTable(
                name: "UserTrademarks");

            migrationBuilder.DropTable(
                name: "Watchlists");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CopyrightRegistrations");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "TrademarkRegistrations");
        }
    }
}
