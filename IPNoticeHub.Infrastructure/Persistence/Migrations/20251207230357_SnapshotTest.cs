using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPNoticeHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "NotificationsEnabled",
                table: "UserTrademarkWatchlists",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false,
                oldComment: "Indicates whether notifications are enabled for this trademark on the watchlist.");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserTrademarkWatchlists",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates whether the user has removed this registration from their watchlist (soft delete).");

            migrationBuilder.AlterColumn<string>(
                name: "InitialStatusText",
                table: "UserTrademarkWatchlists",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "The textual representation of the initial status of the trademark as retrieved from the USPTO database or API.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "InitialStatusDateUtc",
                table: "UserTrademarkWatchlists",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "The UTC date and time when the initial status of the trademark was last updated or retrieved from the USPTO database or API.");

            migrationBuilder.AlterColumn<int>(
                name: "InitialStatusCodeRaw",
                table: "UserTrademarkWatchlists",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "The raw status code of the trademark as retrieved from the USPTO database or API.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddedOnUtc",
                table: "UserTrademarkWatchlists",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "The UTC date and time when the trademark was added to the watchlist.");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserTrademarks",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates whether the user has removed this registration from their collection (soft delete).");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAdded",
                table: "UserTrademarks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "Date when the user added this trademark registration to their account");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserCopyrights",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates whether the user soft-deleted this from their collection/watchlist");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAdded",
                table: "UserCopyrights",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "Date when the user added this copyright registration to their account");

            migrationBuilder.AlterColumn<string>(
                name: "Wordmark",
                table: "TrademarkRegistrations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldComment: "The wordmark or name of the trademark)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatusDateUtc",
                table: "TrademarkRegistrations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Raw status date from source header (e.g., USPTO <status-date>) in UTC");

            migrationBuilder.AlterColumn<int>(
                name: "StatusCodeRaw",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "Raw status code from source header (e.g., USPTO <status-code>)");

            migrationBuilder.AlterColumn<int>(
                name: "StatusCategory",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Current status of the trademark (default is Pending)");

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                table: "TrademarkRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "Original identifier from the source system (USPTO Serial, EUIPO Application, WIPO IRN)");

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Source of the data for the trademark registration");

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "TrademarkRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Registration number of the trademark (optional)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegistrationDate",
                table: "TrademarkRegistrations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Registration date of the trademark (optional)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "TrademarkRegistrations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Unique identifier for the Trademark, generated automatically");

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "TrademarkRegistrations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldComment: "Name of the current owner/s of the trademark");

            migrationBuilder.AlterColumn<string>(
                name: "MarkImageUrl",
                table: "TrademarkRegistrations",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true,
                oldComment: "URL of the image representing the trademark (optional)");

            migrationBuilder.AlterColumn<string>(
                name: "GoodsAndServices",
                table: "TrademarkRegistrations",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldComment: "Description of goods and services associated with the trademark");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FilingDate",
                table: "TrademarkRegistrations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Filing date of the trademark application (optional)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Primary key for the Trademark entity")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "TrademarkId",
                table: "TrademarkEvents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Foreign key referencing the associated TrademarkRegistration");

            migrationBuilder.AlterColumn<string>(
                name: "EventTypeRaw",
                table: "TrademarkEvents",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true,
                oldComment: "Raw event type code as received from the source system");

            migrationBuilder.AlterColumn<int>(
                name: "EventType",
                table: "TrademarkEvents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Enum representing the type of the event");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "TrademarkEvents",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "The date when the event occurred");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TrademarkEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldComment: "Detailed description of the event");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TrademarkEvents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldComment: "Code representing the event type or category");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "TrademarkEvents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Primary key for the TrademarkEvent entity")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ClassNumber",
                table: "TrademarkClassAssignment",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "The Nice Classification number (1–45) assigned to this trademark.");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalFacts",
                table: "LegalDocuments",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfPublication",
                table: "LegalDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoodFaithStatement",
                table: "LegalDocuments",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfringingUrl",
                table: "LegalDocuments",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LetterDate",
                table: "LegalDocuments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NationOfFirstPublication",
                table: "LegalDocuments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientAddress",
                table: "LegalDocuments",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                table: "LegalDocuments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "LegalDocuments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderAddress",
                table: "LegalDocuments",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderEmail",
                table: "LegalDocuments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "LegalDocuments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "YearOfCreation",
                table: "LegalDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "YearOfCreation",
                table: "CopyrightRegistrations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldComment: "Year of creation of the work (if provided)");

            migrationBuilder.AlterColumn<string>(
                name: "TypeOfWork",
                table: "CopyrightRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "Type of work, e.g. Literary Work, Visual Material, Music, Software");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CopyrightRegistrations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldComment: "Title of the work being registered");

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "CopyrightRegistrations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldComment: "Official copyright registration number, e.g. VA0002288838");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "CopyrightRegistrations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "Unique identifier for the Copyright, generated automatically");

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "CopyrightRegistrations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldComment: "Copyright claimant, usually the author or company");

            migrationBuilder.AlterColumn<string>(
                name: "NationOfFirstPublication",
                table: "CopyrightRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Nation of first publication");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfPublication",
                table: "CopyrightRegistrations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Date of publication, if the work has been published");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CopyrightRegistrations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Primary key for the Copyright entity")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.UpdateData(
                table: "UserCopyrights",
                keyColumns: new[] { "ApplicationUserId", "CopyrightRegistrationId" },
                keyValues: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1 },
                column: "DateAdded",
                value: new DateTime(2025, 12, 7, 23, 3, 56, 409, DateTimeKind.Utc).AddTicks(4279));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalFacts",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "DateOfPublication",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "GoodFaithStatement",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "InfringingUrl",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "LetterDate",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "NationOfFirstPublication",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "RecipientAddress",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "SenderAddress",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "SenderEmail",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "YearOfCreation",
                table: "LegalDocuments");

            migrationBuilder.AlterColumn<bool>(
                name: "NotificationsEnabled",
                table: "UserTrademarkWatchlists",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Indicates whether notifications are enabled for this trademark on the watchlist.",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserTrademarkWatchlists",
                type: "bit",
                nullable: false,
                comment: "Indicates whether the user has removed this registration from their watchlist (soft delete).",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "InitialStatusText",
                table: "UserTrademarkWatchlists",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "The textual representation of the initial status of the trademark as retrieved from the USPTO database or API.",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "InitialStatusDateUtc",
                table: "UserTrademarkWatchlists",
                type: "datetime2",
                nullable: true,
                comment: "The UTC date and time when the initial status of the trademark was last updated or retrieved from the USPTO database or API.",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InitialStatusCodeRaw",
                table: "UserTrademarkWatchlists",
                type: "int",
                nullable: true,
                comment: "The raw status code of the trademark as retrieved from the USPTO database or API.",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddedOnUtc",
                table: "UserTrademarkWatchlists",
                type: "datetime2",
                nullable: false,
                comment: "The UTC date and time when the trademark was added to the watchlist.",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserTrademarks",
                type: "bit",
                nullable: false,
                comment: "Indicates whether the user has removed this registration from their collection (soft delete).",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAdded",
                table: "UserTrademarks",
                type: "datetime2",
                nullable: false,
                comment: "Date when the user added this trademark registration to their account",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserCopyrights",
                type: "bit",
                nullable: false,
                comment: "Indicates whether the user soft-deleted this from their collection/watchlist",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAdded",
                table: "UserCopyrights",
                type: "datetime2",
                nullable: false,
                comment: "Date when the user added this copyright registration to their account",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Wordmark",
                table: "TrademarkRegistrations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                comment: "The wordmark or name of the trademark)",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatusDateUtc",
                table: "TrademarkRegistrations",
                type: "datetime2",
                nullable: true,
                comment: "Raw status date from source header (e.g., USPTO <status-date>) in UTC",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StatusCodeRaw",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: true,
                comment: "Raw status code from source header (e.g., USPTO <status-code>)",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StatusCategory",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: false,
                comment: "Current status of the trademark (default is Pending)",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                table: "TrademarkRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "Original identifier from the source system (USPTO Serial, EUIPO Application, WIPO IRN)",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: false,
                comment: "Source of the data for the trademark registration",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "TrademarkRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "Registration number of the trademark (optional)",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegistrationDate",
                table: "TrademarkRegistrations",
                type: "datetime2",
                nullable: true,
                comment: "Registration date of the trademark (optional)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "TrademarkRegistrations",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Unique identifier for the Trademark, generated automatically",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "TrademarkRegistrations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                comment: "Name of the current owner/s of the trademark",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "MarkImageUrl",
                table: "TrademarkRegistrations",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                comment: "URL of the image representing the trademark (optional)",
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GoodsAndServices",
                table: "TrademarkRegistrations",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                comment: "Description of goods and services associated with the trademark",
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FilingDate",
                table: "TrademarkRegistrations",
                type: "datetime2",
                nullable: true,
                comment: "Filing date of the trademark application (optional)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "TrademarkRegistrations",
                type: "int",
                nullable: false,
                comment: "Primary key for the Trademark entity",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "TrademarkId",
                table: "TrademarkEvents",
                type: "int",
                nullable: false,
                comment: "Foreign key referencing the associated TrademarkRegistration",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "EventTypeRaw",
                table: "TrademarkEvents",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                comment: "Raw event type code as received from the source system",
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EventType",
                table: "TrademarkEvents",
                type: "int",
                nullable: false,
                comment: "Enum representing the type of the event",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "TrademarkEvents",
                type: "datetime2",
                nullable: false,
                comment: "The date when the event occurred",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TrademarkEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                comment: "Detailed description of the event",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TrademarkEvents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                comment: "Code representing the event type or category",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "TrademarkEvents",
                type: "int",
                nullable: false,
                comment: "Primary key for the TrademarkEvent entity",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ClassNumber",
                table: "TrademarkClassAssignment",
                type: "int",
                nullable: false,
                comment: "The Nice Classification number (1–45) assigned to this trademark.",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "YearOfCreation",
                table: "CopyrightRegistrations",
                type: "int",
                nullable: true,
                comment: "Year of creation of the work (if provided)",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeOfWork",
                table: "CopyrightRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "Type of work, e.g. Literary Work, Visual Material, Music, Software",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CopyrightRegistrations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                comment: "Title of the work being registered",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "CopyrightRegistrations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                comment: "Official copyright registration number, e.g. VA0002288838",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "CopyrightRegistrations",
                type: "uniqueidentifier",
                nullable: false,
                comment: "Unique identifier for the Copyright, generated automatically",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "CopyrightRegistrations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                comment: "Copyright claimant, usually the author or company",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "NationOfFirstPublication",
                table: "CopyrightRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "Nation of first publication",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfPublication",
                table: "CopyrightRegistrations",
                type: "datetime2",
                nullable: true,
                comment: "Date of publication, if the work has been published",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CopyrightRegistrations",
                type: "int",
                nullable: false,
                comment: "Primary key for the Copyright entity",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.UpdateData(
                table: "UserCopyrights",
                keyColumns: new[] { "ApplicationUserId", "CopyrightRegistrationId" },
                keyValues: new object[] { "2b195b12-9690-46b9-ac8e-50118a7102ea", 1 },
                column: "DateAdded",
                value: new DateTime(2025, 12, 6, 21, 32, 1, 726, DateTimeKind.Utc).AddTicks(3199));
        }
    }
}
