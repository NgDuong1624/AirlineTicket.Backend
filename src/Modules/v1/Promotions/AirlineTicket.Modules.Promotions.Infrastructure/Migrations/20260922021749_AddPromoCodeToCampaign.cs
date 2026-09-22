using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Promotions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoCodeToCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "promo_code",
                schema: "promotions",
                table: "campaigns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "promo_code",
                schema: "promotions",
                table: "campaigns");
        }
    }
}
