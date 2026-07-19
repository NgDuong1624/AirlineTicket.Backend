using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Logs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogTypeAndMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemLog",
                schema: "dbo",
                table: "SystemLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                schema: "dbo",
                table: "SystemLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "dbo",
                table: "SystemLogs",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Type",
                schema: "dbo",
                table: "SystemLogs",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_Type",
                schema: "dbo",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "IsSystemLog",
                schema: "dbo",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "Metadata",
                schema: "dbo",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "dbo",
                table: "SystemLogs");
        }
    }
}
