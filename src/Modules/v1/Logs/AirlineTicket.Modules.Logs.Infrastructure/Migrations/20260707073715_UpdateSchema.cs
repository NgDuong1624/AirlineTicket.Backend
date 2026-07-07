using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Logs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_AirlineId_CreatedAt",
                schema: "dbo",
                table: "SystemLogs",
                columns: new[] { "AirlineId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_CreatedAt",
                schema: "dbo",
                table: "SystemLogs",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_AirlineId_CreatedAt",
                schema: "dbo",
                table: "SystemLogs");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_CreatedAt",
                schema: "dbo",
                table: "SystemLogs");
        }
    }
}
