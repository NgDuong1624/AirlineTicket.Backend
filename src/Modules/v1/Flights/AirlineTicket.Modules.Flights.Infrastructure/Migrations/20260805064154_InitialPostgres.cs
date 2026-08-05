using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "flights");

            migrationBuilder.CreateTable(
                name: "aircraft_models",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    manufacturer = table.Column<string>(type: "text", nullable: false),
                    total_seats = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aircraft_models", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "airlines",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    iata_code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    base_country = table.Column<string>(type: "text", nullable: true),
                    api_endpoint = table.Column<string>(type: "text", nullable: true),
                    api_key = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    support_email = table.Column<string>(type: "text", nullable: true),
                    support_phone = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_airlines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "airports",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    iata_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name_en = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name_vi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city_vi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_airports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "aircraft_model_seat_templates",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aircraft_model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_number = table.Column<string>(type: "text", nullable: false),
                    seat_row = table.Column<string>(type: "text", nullable: false),
                    seat_column = table.Column<string>(type: "text", nullable: false),
                    seat_class = table.Column<int>(type: "integer", nullable: false),
                    is_extra_legroom = table.Column<bool>(type: "boolean", nullable: false),
                    price_multiplier = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aircraft_model_seat_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_aircraft_model_seat_templates_aircraft_models_aircraft_mode",
                        column: x => x.aircraft_model_id,
                        principalSchema: "flights",
                        principalTable: "aircraft_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "airplanes",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    airline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aircraft_model_id = table.Column<Guid>(type: "uuid", nullable: true),
                    model = table.Column<string>(type: "text", nullable: false),
                    registration_number = table.Column<string>(type: "text", nullable: false),
                    total_capacity = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_airplanes", x => x.id);
                    table.ForeignKey(
                        name: "fk_airplanes_aircraft_models_aircraft_model_id",
                        column: x => x.aircraft_model_id,
                        principalSchema: "flights",
                        principalTable: "aircraft_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_airplanes_airlines_airline_id",
                        column: x => x.airline_id,
                        principalSchema: "flights",
                        principalTable: "airlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "routes",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    airline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin_airport_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_airport_id = table.Column<Guid>(type: "uuid", nullable: false),
                    distance_km = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    estimated_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_routes", x => x.id);
                    table.ForeignKey(
                        name: "fk_routes_airlines_airline_id",
                        column: x => x.airline_id,
                        principalSchema: "flights",
                        principalTable: "airlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_routes_airports_destination_airport_id",
                        column: x => x.destination_airport_id,
                        principalSchema: "flights",
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_routes_airports_origin_airport_id",
                        column: x => x.origin_airport_id,
                        principalSchema: "flights",
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "airplane_seats",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    airplane_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_number = table.Column<string>(type: "text", nullable: false),
                    seat_row = table.Column<string>(type: "text", nullable: false),
                    seat_column = table.Column<string>(type: "text", nullable: false),
                    seat_class = table.Column<int>(type: "integer", nullable: false),
                    is_extra_legroom = table.Column<bool>(type: "boolean", nullable: false),
                    price_multiplier = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_airplane_seats", x => x.id);
                    table.ForeignKey(
                        name: "fk_airplane_seats_airplanes_airplane_id",
                        column: x => x.airplane_id,
                        principalSchema: "flights",
                        principalTable: "airplanes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "flights",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    airplane_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_number = table.Column<string>(type: "text", nullable: false),
                    departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    arrival_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    base_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flights", x => x.id);
                    table.ForeignKey(
                        name: "fk_flights_airplanes_airplane_id",
                        column: x => x.airplane_id,
                        principalSchema: "flights",
                        principalTable: "airplanes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_flights_routes_route_id",
                        column: x => x.route_id,
                        principalSchema: "flights",
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "flight_seats",
                schema: "flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_number = table.Column<string>(type: "text", nullable: false),
                    seat_class = table.Column<int>(type: "integer", nullable: false),
                    price_override = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    is_extra_legroom = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flight_seats", x => x.id);
                    table.ForeignKey(
                        name: "fk_flight_seats_flights_flight_id",
                        column: x => x.flight_id,
                        principalSchema: "flights",
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aircraft_model_seat_templates_aircraft_model_id",
                schema: "flights",
                table: "aircraft_model_seat_templates",
                column: "aircraft_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_airplane_seats_airplane_id",
                schema: "flights",
                table: "airplane_seats",
                column: "airplane_id");

            migrationBuilder.CreateIndex(
                name: "ix_airplanes_aircraft_model_id",
                schema: "flights",
                table: "airplanes",
                column: "aircraft_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_airplanes_airline_id",
                schema: "flights",
                table: "airplanes",
                column: "airline_id");

            migrationBuilder.CreateIndex(
                name: "ix_airports_iata_code",
                schema: "flights",
                table: "airports",
                column: "iata_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flight_seats_flight_id_is_available",
                schema: "flights",
                table: "flight_seats",
                columns: new[] { "flight_id", "is_available" });

            migrationBuilder.CreateIndex(
                name: "ix_flights_airplane_id_departure_time",
                schema: "flights",
                table: "flights",
                columns: new[] { "airplane_id", "departure_time" });

            migrationBuilder.CreateIndex(
                name: "ix_flights_departure_time_arrival_time",
                schema: "flights",
                table: "flights",
                columns: new[] { "departure_time", "arrival_time" });

            migrationBuilder.CreateIndex(
                name: "ix_flights_route_id_departure_time",
                schema: "flights",
                table: "flights",
                columns: new[] { "route_id", "departure_time" });

            migrationBuilder.CreateIndex(
                name: "ix_routes_airline_id",
                schema: "flights",
                table: "routes",
                column: "airline_id");

            migrationBuilder.CreateIndex(
                name: "ix_routes_destination_airport_id",
                schema: "flights",
                table: "routes",
                column: "destination_airport_id");

            migrationBuilder.CreateIndex(
                name: "ix_routes_origin_airport_id",
                schema: "flights",
                table: "routes",
                column: "origin_airport_id");

            migrationBuilder.CreateIndex(
                name: "ix_routes_origin_airport_id_destination_airport_id",
                schema: "flights",
                table: "routes",
                columns: new[] { "origin_airport_id", "destination_airport_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft_model_seat_templates",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "airplane_seats",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "flight_seats",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "flights",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "airplanes",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "routes",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "aircraft_models",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "airlines",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "airports",
                schema: "flights");
        }
    }
}
