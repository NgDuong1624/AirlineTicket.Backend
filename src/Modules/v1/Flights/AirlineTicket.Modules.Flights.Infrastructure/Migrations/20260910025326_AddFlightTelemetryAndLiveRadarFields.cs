using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightTelemetryAndLiveRadarFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "actual_arrival_time",
                schema: "flights",
                table: "flights",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "actual_departure_time",
                schema: "flights",
                table: "flights",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "arrival_gate",
                schema: "flights",
                table: "flights",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "baggage_carousel",
                schema: "flights",
                table: "flights",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "delay_minutes",
                schema: "flights",
                table: "flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "departure_gate",
                schema: "flights",
                table: "flights",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "flight_telemetry",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    altitude_feet = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    speed_knots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    heading_degrees = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    progress_percentage = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    estimated_arrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flight_telemetry", x => x.id);
                    table.ForeignKey(
                        name: "fk_flight_telemetry_flights_flight_id",
                        column: x => x.flight_id,
                        principalSchema: "flights",
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_flight_telemetry_flight_id_recorded_at",
                schema: "flights",
                table: "flight_telemetry",
                columns: new[] { "flight_id", "recorded_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_telemetry",
                schema: "flights");

            migrationBuilder.DropColumn(
                name: "actual_arrival_time",
                schema: "flights",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "actual_departure_time",
                schema: "flights",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "arrival_gate",
                schema: "flights",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "baggage_carousel",
                schema: "flights",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "delay_minutes",
                schema: "flights",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "departure_gate",
                schema: "flights",
                table: "flights");
        }
    }
}
