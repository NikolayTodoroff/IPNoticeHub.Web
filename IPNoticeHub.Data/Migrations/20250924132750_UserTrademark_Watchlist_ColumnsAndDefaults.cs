using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IPNoticeHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserTrademark_Watchlist_ColumnsAndDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCopyright");

            migrationBuilder.DropTable(
                name: "UserTrademark");

            migrationBuilder.RenameIndex(
                name: "IX_TrademarkRegistrations_Source_SourceId",
                table: "TrademarkRegistrations",
                newName: "UX_Trademark_Source_SourceId");

            migrationBuilder.AlterColumn<string>(
                name: "StatusDetail",
                table: "TrademarkRegistrations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "GoodsAndServices",
                table: "TrademarkRegistrations",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                comment: "Description of goods and services associated with the trademark",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldComment: "Description of goods and services associated with the trademark");

            migrationBuilder.AddColumn<int>(
                name: "StatusCodeRaw",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: true,
                comment: "Raw status code from source header (e.g., USPTO <status-code>)");

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusDateUtc",
                table: "TrademarkRegistrations",
                type: "datetime2",
                nullable: true,
                comment: "Raw status date from source header (e.g., USPTO <status-date>) in UTC");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "CopyrightRegistrations",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Unique identifier for the Copyright, generated automatically",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Public identifier for the copyright (e.g. Registration Number)");

            migrationBuilder.CreateTable(
                name: "UserCopyrights",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CopyrightRegistrationId = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date when the user added this copyright registration to their account"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the user soft-deleted this from their collection/watchlist")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCopyrights", x => new { x.ApplicationUserId, x.CopyrightRegistrationId });
                    table.ForeignKey(
                        name: "FK_UserCopyrights_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCopyrights_CopyrightRegistrations_CopyrightRegistrationId",
                        column: x => x.CopyrightRegistrationId,
                        principalTable: "CopyrightRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTrademarks",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrademarkRegistrationId = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date when the user added this trademark registration to their account"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the user has removed this registration from their collection or watchlist (soft delete)."),
                    AddedToWatchlist = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the trademark is on a watchlist"),
                    WatchlistNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicates whether notifications are enabled for this trademark on the watchlist."),
                    WatchlistAddedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "The UTC date and time when the trademark was added to the watchlist."),
                    WatchlistInitialStatusCodeRaw = table.Column<int>(type: "int", nullable: true, comment: "The raw status code of the trademark as retrieved from the USPTO database or API."),
                    WatchlistInitialStatusText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "The textual representation of the initial status of the trademark as retrieved from the USPTO database or API."),
                    WatchlistInitialStatusDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Indicates whether the initial status date of the trademark was retrieved from the USPTO database or API.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrademarks", x => new { x.ApplicationUserId, x.TrademarkRegistrationId });
                    table.ForeignKey(
                        name: "FK_UserTrademarks_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTrademarks_TrademarkRegistrations_TrademarkRegistrationId",
                        column: x => x.TrademarkRegistrationId,
                        principalTable: "TrademarkRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "TrademarkRegistrations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "StatusCodeRaw", "StatusDateUtc" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "TrademarkRegistrations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "StatusCodeRaw", "StatusDateUtc" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "UserCopyrights",
                columns: new[] { "ApplicationUserId", "CopyrightRegistrationId", "DateAdded", "IsDeleted" },
                values: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1, new DateTime(2025, 9, 24, 13, 27, 48, 364, DateTimeKind.Utc).AddTicks(8503), false });

            migrationBuilder.InsertData(
                table: "UserTrademarks",
                columns: new[] { "ApplicationUserId", "TrademarkRegistrationId", "AddedToWatchlist", "DateAdded", "IsDeleted", "WatchlistAddedOnUtc", "WatchlistInitialStatusCodeRaw", "WatchlistInitialStatusDateUtc", "WatchlistInitialStatusText" },
                values: new object[,]
                {
                    { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1, true, new DateTime(2025, 9, 24, 13, 27, 48, 364, DateTimeKind.Utc).AddTicks(8426), false, null, null, null, null },
                    { "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b", 2, false, new DateTime(2025, 9, 24, 13, 27, 48, 364, DateTimeKind.Utc).AddTicks(8430), false, null, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkRegistrations_RegistrationNumber",
                table: "TrademarkRegistrations",
                column: "RegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrademarkRegistrations_SourceId",
                table: "TrademarkRegistrations",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCopyrights_ApplicationUserId_CopyrightRegistrationId",
                table: "UserCopyrights",
                columns: new[] { "ApplicationUserId", "CopyrightRegistrationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCopyrights_CopyrightRegistrationId",
                table: "UserCopyrights",
                column: "CopyrightRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrademarks_TrademarkRegistrationId",
                table: "UserTrademarks",
                column: "TrademarkRegistrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCopyrights");

            migrationBuilder.DropTable(
                name: "UserTrademarks");

            migrationBuilder.DropIndex(
                name: "IX_TrademarkRegistrations_RegistrationNumber",
                table: "TrademarkRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_TrademarkRegistrations_SourceId",
                table: "TrademarkRegistrations");

            migrationBuilder.DropColumn(
                name: "StatusCodeRaw",
                table: "TrademarkRegistrations");

            migrationBuilder.DropColumn(
                name: "StatusDateUtc",
                table: "TrademarkRegistrations");

            migrationBuilder.RenameIndex(
                name: "UX_Trademark_Source_SourceId",
                table: "TrademarkRegistrations",
                newName: "IX_TrademarkRegistrations_Source_SourceId");

            migrationBuilder.AlterColumn<string>(
                name: "StatusDetail",
                table: "TrademarkRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "GoodsAndServices",
                table: "TrademarkRegistrations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                comment: "Description of goods and services associated with the trademark",
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldComment: "Description of goods and services associated with the trademark");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "CopyrightRegistrations",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Public identifier for the copyright (e.g. Registration Number)",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Unique identifier for the Copyright, generated automatically");

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

            migrationBuilder.CreateIndex(
                name: "IX_UserCopyright_CopyrightRegistrationId",
                table: "UserCopyright",
                column: "CopyrightRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrademark_TrademarkRegistrationId",
                table: "UserTrademark",
                column: "TrademarkRegistrationId");
        }
    }
}
