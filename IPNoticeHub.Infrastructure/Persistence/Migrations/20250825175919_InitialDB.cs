using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace IPNoticeHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDB : Migration
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
                    Id = table.Column<int>(type: "int", nullable: false, comment: "Primary key for the Copyright entity")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Public identifier for the copyright (e.g. Registration Number)"),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "Official copyright registration number, e.g. VA0002288838"),
                    TypeOfWork = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Type of work, e.g. Literary Work, Visual Material, Music, Software"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, comment: "Title of the work being registered"),
                    YearOfCreation = table.Column<int>(type: "int", nullable: true, comment: "Year of creation of the work (if provided)"),
                    DateOfPublication = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Date of publication, if the work has been published"),
                    Owner = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Copyright claimant, usually the author or company"),
                    NationOfFirstPublication = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Nation of first publication")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CopyrightRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, comment: "Primary key for the Trademark entity")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Unique identifier for the Trademark, generated automatically"),
                    Wordmark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, comment: "The wordmark or name of the trademark)"),
                    SourceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Original identifier from the source system (USPTO Serial, EUIPO Application, WIPO IRN)"),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Registration number of the trademark (optional)"),
                    GoodsAndServices = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, comment: "Description of goods and services associated with the trademark"),
                    Owner = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Name of the current owner/s of the trademark"),
                    StatusCategory = table.Column<int>(type: "int", nullable: false, comment: "Current status of the trademark (default is Pending)"),
                    StatusDetail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilingDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Filing date of the trademark application (optional)"),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Registration date of the trademark (optional)"),
                    MarkImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true, comment: "URL of the image representing the trademark (optional)"),
                    Source = table.Column<int>(type: "int", nullable: false, comment: "Source of the data for the trademark registration")
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
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
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
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
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
                name: "UserCopyright",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CopyrightRegistrationId = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date when the user added this copyright registration to their account"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the user soft-deleted this from their collection/watchlist")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCopyright", x => new { x.ApplicationUserId, x.CopyrightRegistrationId });
                    table.ForeignKey(
                        name: "FK_UserCopyright_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCopyright_CopyrightRegistrations_CopyrightRegistrationId",
                        column: x => x.CopyrightRegistrationId,
                        principalTable: "CopyrightRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrademarkClassAssignment",
                columns: table => new
                {
                    ClassNumber = table.Column<int>(type: "int", nullable: false, comment: "The Nice Classification number (1–45) assigned to this trademark."),
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
                    Id = table.Column<int>(type: "int", nullable: false, comment: "Primary key for the TrademarkEvent entity")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrademarkId = table.Column<int>(type: "int", nullable: false, comment: "Foreign key referencing the associated TrademarkRegistration"),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "The date when the event occurred"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Code representing the event type or category"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Detailed description of the event"),
                    EventTypeRaw = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true, comment: "Raw event type code as received from the source system"),
                    EventType = table.Column<int>(type: "int", nullable: false, comment: "Enum representing the type of the event")
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
                name: "UserTrademark",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrademarkRegistrationId = table.Column<int>(type: "int", nullable: false),
                    AddedToWatchlist = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the trademark is on a watchlist"),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date when the user added this trademark registration to their account"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the user has removed this registration from their collection or watchlist (soft delete).")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrademark", x => new { x.ApplicationUserId, x.TrademarkRegistrationId });
                    table.ForeignKey(
                        name: "FK_UserTrademark_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTrademark_TrademarkRegistrations_TrademarkRegistrationId",
                        column: x => x.TrademarkRegistrationId,
                        principalTable: "TrademarkRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_TrademarkEvents_TrademarkId",
                table: "TrademarkEvents",
                column: "TrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkRegistrations_Source_SourceId",
                table: "TrademarkRegistrations",
                columns: new[] { "Source", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCopyright_CopyrightRegistrationId",
                table: "UserCopyright",
                column: "CopyrightRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrademark_TrademarkRegistrationId",
                table: "UserTrademark",
                column: "TrademarkRegistrationId");
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
                name: "TrademarkClassAssignment");

            migrationBuilder.DropTable(
                name: "TrademarkEvents");

            migrationBuilder.DropTable(
                name: "UserCopyright");

            migrationBuilder.DropTable(
                name: "UserTrademark");

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
