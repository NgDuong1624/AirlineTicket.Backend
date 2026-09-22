using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Bookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponFieldsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "coupon_code",
                schema: "bookings",
                table: "bookings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                schema: "bookings",
                table: "bookings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coupon_code",
                schema: "bookings",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                schema: "bookings",
                table: "bookings");
        }
    }
}
