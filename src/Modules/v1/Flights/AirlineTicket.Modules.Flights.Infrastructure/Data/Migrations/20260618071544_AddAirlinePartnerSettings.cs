using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAirlinePartnerSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "flights",
                table: "Airlines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportEmail",
                schema: "flights",
                table: "Airlines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportPhone",
                schema: "flights",
                table: "Airlines",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.DropColumn(
                name: "SupportEmail",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.DropColumn(
                name: "SupportPhone",
                schema: "flights",
                table: "Airlines");
        }
    }
}
