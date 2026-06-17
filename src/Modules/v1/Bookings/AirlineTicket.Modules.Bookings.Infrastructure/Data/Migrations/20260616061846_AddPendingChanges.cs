using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "bookings",
                table: "Payments");

            migrationBuilder.AddColumn<DateTime>(
                name: "BoardingTime",
                schema: "bookings",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gate",
                schema: "bookings",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessful",
                schema: "bookings",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProviderStatus",
                schema: "bookings",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawResponse",
                schema: "bookings",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                schema: "bookings",
                table: "Passengers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                schema: "bookings",
                table: "Passengers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportExpiryDate",
                schema: "bookings",
                table: "Passengers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                schema: "bookings",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                schema: "bookings",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "bookings",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "bookings",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SpecialRequests",
                schema: "bookings",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "bookings",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoardingTime",
                schema: "bookings",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Gate",
                schema: "bookings",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsSuccessful",
                schema: "bookings",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderStatus",
                schema: "bookings",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RawResponse",
                schema: "bookings",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "bookings",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "Nationality",
                schema: "bookings",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "PassportExpiryDate",
                schema: "bookings",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                schema: "bookings",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                schema: "bookings",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "bookings",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "bookings",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SpecialRequests",
                schema: "bookings",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "bookings",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "bookings",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
