using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightPriceHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flight_price_histories",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    seat_class = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Economy"),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flight_price_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_flight_price_histories_flights_flight_id",
                        column: x => x.flight_id,
                        principalSchema: "flights",
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_flight_price_histories_routes_route_id",
                        column: x => x.route_id,
                        principalSchema: "flights",
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_flight_price_history_flight_date",
                schema: "flights",
                table: "flight_price_histories",
                columns: new[] { "flight_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "idx_flight_price_history_route_date",
                schema: "flights",
                table: "flight_price_histories",
                columns: new[] { "route_id", "recorded_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_price_histories",
                schema: "flights");
        }
    }
}
