using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerScopedPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AirlineId",
                schema: "promotions",
                table: "Coupons",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AirlineId",
                schema: "promotions",
                table: "Campaigns",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirlineId",
                schema: "promotions",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "AirlineId",
                schema: "promotions",
                table: "Campaigns");
        }
    }
}
