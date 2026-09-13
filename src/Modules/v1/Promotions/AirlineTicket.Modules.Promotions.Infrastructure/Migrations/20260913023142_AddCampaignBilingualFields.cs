using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Promotions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignBilingualFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "title",
                schema: "promotions",
                table: "campaigns",
                newName: "title_vi");

            migrationBuilder.RenameColumn(
                name: "content",
                schema: "promotions",
                table: "campaigns",
                newName: "content_vi");

            migrationBuilder.AddColumn<string>(
                name: "content_en",
                schema: "promotions",
                table: "campaigns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title_en",
                schema: "promotions",
                table: "campaigns",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE promotions.campaigns SET title_en = title_vi, content_en = content_vi WHERE title_en = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_en",
                schema: "promotions",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "title_en",
                schema: "promotions",
                table: "campaigns");

            migrationBuilder.RenameColumn(
                name: "title_vi",
                schema: "promotions",
                table: "campaigns",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "content_vi",
                schema: "promotions",
                table: "campaigns",
                newName: "content");
        }
    }
}
