using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPNoticeHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class WatchlistEntityCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTrademarks_AspNetUsers_ApplicationUserId",
                table: "UserTrademarks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTrademarks_TrademarkRegistrations_TrademarkRegistrationId",
                table: "UserTrademarks");

            migrationBuilder.DropColumn(
                name: "AddedToWatchlist",
                table: "UserTrademarks");

            migrationBuilder.DropColumn(
                name: "WatchlistAddedOnUtc",
                table: "UserTrademarks");

            migrationBuilder.DropColumn(
                name: "WatchlistInitialStatusCodeRaw",
                table: "UserTrademarks");

            migrationBuilder.DropColumn(
                name: "WatchlistInitialStatusDateUtc",
                table: "UserTrademarks");

            migrationBuilder.DropColumn(
                name: "WatchlistInitialStatusText",
                table: "UserTrademarks");

            migrationBuilder.DropColumn(
                name: "WatchlistNotificationsEnabled",
                table: "UserTrademarks");

            migrationBuilder.RenameColumn(
                name: "TrademarkRegistrationId",
                table: "UserTrademarks",
                newName: "TrademarkId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "UserTrademarks",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTrademarks_TrademarkRegistrationId",
                table: "UserTrademarks",
                newName: "IX_UserTrademarks_TrademarkId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserTrademarks",
                type: "bit",
                nullable: false,
                comment: "Indicates whether the user has removed this registration from their collection (soft delete).",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates whether the user has removed this registration from their collection or watchlist (soft delete).");

            migrationBuilder.CreateTable(
                name: "UserTrademarkWatchlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrademarkId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the user has removed this registration from their watchlist (soft delete)."),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicates whether notifications are enabled for this trademark on the watchlist."),
                    AddedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "The UTC date and time when the trademark was added to the watchlist."),
                    InitialStatusCodeRaw = table.Column<int>(type: "int", nullable: true, comment: "The raw status code of the trademark as retrieved from the USPTO database or API."),
                    InitialStatusText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "The textual representation of the initial status of the trademark as retrieved from the USPTO database or API."),
                    InitialStatusDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "The UTC date and time when the initial status of the trademark was last updated or retrieved from the USPTO database or API.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrademarkWatchlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTrademarkWatchlists_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTrademarkWatchlists_TrademarkRegistrations_TrademarkId",
                        column: x => x.TrademarkId,
                        principalTable: "TrademarkRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UserCopyrights",
                keyColumns: new[] { "ApplicationUserId", "CopyrightRegistrationId" },
                keyValues: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1 },
                column: "DateAdded",
                value: new DateTime(2025, 9, 27, 14, 20, 5, 572, DateTimeKind.Utc).AddTicks(9296));

            migrationBuilder.CreateIndex(
                name: "IX_UserTrademarkWatchlists_TrademarkId",
                table: "UserTrademarkWatchlists",
                column: "TrademarkId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrademarkWatchlists_UserId_TrademarkId",
                table: "UserTrademarkWatchlists",
                columns: new[] { "UserId", "TrademarkId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrademarks_AspNetUsers_UserId",
                table: "UserTrademarks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrademarks_TrademarkRegistrations_TrademarkId",
                table: "UserTrademarks",
                column: "TrademarkId",
                principalTable: "TrademarkRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTrademarks_AspNetUsers_UserId",
                table: "UserTrademarks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTrademarks_TrademarkRegistrations_TrademarkId",
                table: "UserTrademarks");

            migrationBuilder.DropTable(
                name: "UserTrademarkWatchlists");

            migrationBuilder.RenameColumn(
                name: "TrademarkId",
                table: "UserTrademarks",
                newName: "TrademarkRegistrationId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserTrademarks",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTrademarks_TrademarkId",
                table: "UserTrademarks",
                newName: "IX_UserTrademarks_TrademarkRegistrationId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserTrademarks",
                type: "bit",
                nullable: false,
                comment: "Indicates whether the user has removed this registration from their collection or watchlist (soft delete).",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates whether the user has removed this registration from their collection (soft delete).");

            migrationBuilder.AddColumn<bool>(
                name: "AddedToWatchlist",
                table: "UserTrademarks",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Indicates whether the trademark is on a watchlist");

            migrationBuilder.AddColumn<DateTime>(
                name: "WatchlistAddedOnUtc",
                table: "UserTrademarks",
                type: "datetime2",
                nullable: true,
                comment: "The UTC date and time when the trademark was added to the watchlist.");

            migrationBuilder.AddColumn<int>(
                name: "WatchlistInitialStatusCodeRaw",
                table: "UserTrademarks",
                type: "int",
                nullable: true,
                comment: "The raw status code of the trademark as retrieved from the USPTO database or API.");

            migrationBuilder.AddColumn<DateTime>(
                name: "WatchlistInitialStatusDateUtc",
                table: "UserTrademarks",
                type: "datetime2",
                nullable: true,
                comment: "Indicates whether the initial status date of the trademark was retrieved from the USPTO database or API.");

            migrationBuilder.AddColumn<string>(
                name: "WatchlistInitialStatusText",
                table: "UserTrademarks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "The textual representation of the initial status of the trademark as retrieved from the USPTO database or API.");

            migrationBuilder.AddColumn<bool>(
                name: "WatchlistNotificationsEnabled",
                table: "UserTrademarks",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Indicates whether notifications are enabled for this trademark on the watchlist.");

            migrationBuilder.UpdateData(
                table: "UserCopyrights",
                keyColumns: new[] { "ApplicationUserId", "CopyrightRegistrationId" },
                keyValues: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1 },
                column: "DateAdded",
                value: new DateTime(2025, 9, 24, 13, 50, 8, 780, DateTimeKind.Utc).AddTicks(9903));

            migrationBuilder.UpdateData(
                table: "UserTrademarks",
                keyColumns: new[] { "ApplicationUserId", "TrademarkRegistrationId" },
                keyValues: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1 },
                columns: new[] { "AddedToWatchlist", "WatchlistAddedOnUtc", "WatchlistInitialStatusCodeRaw", "WatchlistInitialStatusDateUtc", "WatchlistInitialStatusText" },
                values: new object[] { true, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "UserTrademarks",
                keyColumns: new[] { "ApplicationUserId", "TrademarkRegistrationId" },
                keyValues: new object[] { "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b", 2 },
                columns: new[] { "WatchlistAddedOnUtc", "WatchlistInitialStatusCodeRaw", "WatchlistInitialStatusDateUtc", "WatchlistInitialStatusText" },
                values: new object[] { null, null, null, null });

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrademarks_AspNetUsers_ApplicationUserId",
                table: "UserTrademarks",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrademarks_TrademarkRegistrations_TrademarkRegistrationId",
                table: "UserTrademarks",
                column: "TrademarkRegistrationId",
                principalTable: "TrademarkRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
