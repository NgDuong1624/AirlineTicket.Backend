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
                name: "AircraftModels",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airlines",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IataCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    BaseCountry = table.Column<string>(type: "text", nullable: true),
                    ApiEndpoint = table.Column<string>(type: "text", nullable: true),
                    ApiKey = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    SupportEmail = table.Column<string>(type: "text", nullable: true),
                    SupportPhone = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airlines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IataCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameVi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CityEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CityVi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AircraftModelSeatTemplates",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AircraftModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatNumber = table.Column<string>(type: "text", nullable: false),
                    SeatRow = table.Column<string>(type: "text", nullable: false),
                    SeatColumn = table.Column<string>(type: "text", nullable: false),
                    SeatClass = table.Column<int>(type: "integer", nullable: false),
                    IsExtraLegroom = table.Column<bool>(type: "boolean", nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftModelSeatTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AircraftModelSeatTemplates_AircraftModels_AircraftModelId",
                        column: x => x.AircraftModelId,
                        principalSchema: "flights",
                        principalTable: "AircraftModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Airplanes",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AirlineId = table.Column<Guid>(type: "uuid", nullable: false),
                    AircraftModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "text", nullable: false),
                    TotalCapacity = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airplanes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Airplanes_AircraftModels_AircraftModelId",
                        column: x => x.AircraftModelId,
                        principalSchema: "flights",
                        principalTable: "AircraftModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Airplanes_Airlines_AirlineId",
                        column: x => x.AirlineId,
                        principalSchema: "flights",
                        principalTable: "Airlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AirlineId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginAirportId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationAirportId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Airlines_AirlineId",
                        column: x => x.AirlineId,
                        principalSchema: "flights",
                        principalTable: "Airlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Airports_DestinationAirportId",
                        column: x => x.DestinationAirportId,
                        principalSchema: "flights",
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Airports_OriginAirportId",
                        column: x => x.OriginAirportId,
                        principalSchema: "flights",
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AirplaneSeats",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AirplaneId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatNumber = table.Column<string>(type: "text", nullable: false),
                    SeatRow = table.Column<string>(type: "text", nullable: false),
                    SeatColumn = table.Column<string>(type: "text", nullable: false),
                    SeatClass = table.Column<int>(type: "integer", nullable: false),
                    IsExtraLegroom = table.Column<bool>(type: "boolean", nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirplaneSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AirplaneSeats_Airplanes_AirplaneId",
                        column: x => x.AirplaneId,
                        principalSchema: "flights",
                        principalTable: "Airplanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    AirplaneId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlightNumber = table.Column<string>(type: "text", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flights_Airplanes_AirplaneId",
                        column: x => x.AirplaneId,
                        principalSchema: "flights",
                        principalTable: "Airplanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Routes_RouteId",
                        column: x => x.RouteId,
                        principalSchema: "flights",
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlightSeats",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlightId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatNumber = table.Column<string>(type: "text", nullable: false),
                    SeatClass = table.Column<int>(type: "integer", nullable: false),
                    PriceOverride = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsExtraLegroom = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightSeats_Flights_FlightId",
                        column: x => x.FlightId,
                        principalSchema: "flights",
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AircraftModelSeatTemplates_AircraftModelId",
                schema: "flights",
                table: "AircraftModelSeatTemplates",
                column: "AircraftModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Airplanes_AircraftModelId",
                schema: "flights",
                table: "Airplanes",
                column: "AircraftModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Airplanes_AirlineId",
                schema: "flights",
                table: "Airplanes",
                column: "AirlineId");

            migrationBuilder.CreateIndex(
                name: "IX_AirplaneSeats_AirplaneId",
                schema: "flights",
                table: "AirplaneSeats",
                column: "AirplaneId");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_IataCode",
                schema: "flights",
                table: "Airports",
                column: "IataCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AirplaneId",
                schema: "flights",
                table: "Flights",
                column: "AirplaneId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_RouteId",
                schema: "flights",
                table: "Flights",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSeats_FlightId",
                schema: "flights",
                table: "FlightSeats",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_AirlineId",
                schema: "flights",
                table: "Routes",
                column: "AirlineId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DestinationAirportId",
                schema: "flights",
                table: "Routes",
                column: "DestinationAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_OriginAirportId",
                schema: "flights",
                table: "Routes",
                column: "OriginAirportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AircraftModelSeatTemplates",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "AirplaneSeats",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "FlightSeats",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "Flights",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "Airplanes",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "Routes",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "AircraftModels",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "Airlines",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "Airports",
                schema: "flights");
        }
    }
}
