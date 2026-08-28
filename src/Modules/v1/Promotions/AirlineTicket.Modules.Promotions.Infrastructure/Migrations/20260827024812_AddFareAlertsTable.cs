using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Promotions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFareAlertsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fare_alerts",
                schema: "promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin_airport_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_airport_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departure_date = table.Column<DateOnly>(type: "date", nullable: false),
                    return_date = table.Column<DateOnly>(type: "date", nullable: true),
                    target_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    current_lowest_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    last_notified_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "VND"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_notified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fare_alerts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_fare_alerts_active_route",
                schema: "promotions",
                table: "fare_alerts",
                columns: new[] { "origin_airport_id", "destination_airport_id", "departure_date", "is_active" });

            migrationBuilder.CreateIndex(
                name: "idx_fare_alerts_user_active",
                schema: "promotions",
                table: "fare_alerts",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "uq_fare_alerts_user_route",
                schema: "promotions",
                table: "fare_alerts",
                columns: new[] { "user_id", "origin_airport_id", "destination_airport_id", "departure_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fare_alerts",
                schema: "promotions");
        }
    }
}
