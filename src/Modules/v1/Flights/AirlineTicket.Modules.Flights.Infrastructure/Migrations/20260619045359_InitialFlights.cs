using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Airlines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IataCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseCountry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airlines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IataCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameVi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CityEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityVi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airplanes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirlineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airplanes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Airplanes_Airlines_AirlineId",
                        column: x => x.AirlineId,
                        principalSchema: "dbo",
                        principalTable: "Airlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirlineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginAirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationAirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AirportId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Airlines_AirlineId",
                        column: x => x.AirlineId,
                        principalSchema: "dbo",
                        principalTable: "Airlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Airports_AirportId",
                        column: x => x.AirportId,
                        principalSchema: "dbo",
                        principalTable: "Airports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Routes_Airports_AirportId1",
                        column: x => x.AirportId1,
                        principalSchema: "dbo",
                        principalTable: "Airports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Routes_Airports_DestinationAirportId",
                        column: x => x.DestinationAirportId,
                        principalSchema: "dbo",
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Airports_OriginAirportId",
                        column: x => x.OriginAirportId,
                        principalSchema: "dbo",
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AirplaneSeats",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirplaneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatRow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatColumn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsExtraLegroom = table.Column<bool>(type: "bit", nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirplaneSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AirplaneSeats_Airplanes_AirplaneId",
                        column: x => x.AirplaneId,
                        principalSchema: "dbo",
                        principalTable: "Airplanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirplaneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AirplaneId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RouteId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flights_Airplanes_AirplaneId",
                        column: x => x.AirplaneId,
                        principalSchema: "dbo",
                        principalTable: "Airplanes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airplanes_AirplaneId1",
                        column: x => x.AirplaneId1,
                        principalSchema: "dbo",
                        principalTable: "Airplanes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Flights_Routes_RouteId",
                        column: x => x.RouteId,
                        principalSchema: "dbo",
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Routes_RouteId1",
                        column: x => x.RouteId1,
                        principalSchema: "dbo",
                        principalTable: "Routes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FlightSeats",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatClass = table.Column<int>(type: "int", nullable: false),
                    PriceOverride = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsExtraLegroom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightSeats_Flights_FlightId",
                        column: x => x.FlightId,
                        principalSchema: "dbo",
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Airplanes_AirlineId",
                schema: "dbo",
                table: "Airplanes",
                column: "AirlineId");

            migrationBuilder.CreateIndex(
                name: "IX_AirplaneSeats_AirplaneId",
                schema: "dbo",
                table: "AirplaneSeats",
                column: "AirplaneId");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_IataCode",
                schema: "dbo",
                table: "Airports",
                column: "IataCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AirplaneId",
                schema: "dbo",
                table: "Flights",
                column: "AirplaneId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AirplaneId1",
                schema: "dbo",
                table: "Flights",
                column: "AirplaneId1");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_RouteId",
                schema: "dbo",
                table: "Flights",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_RouteId1",
                schema: "dbo",
                table: "Flights",
                column: "RouteId1");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSeats_FlightId",
                schema: "dbo",
                table: "FlightSeats",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_AirlineId",
                schema: "dbo",
                table: "Routes",
                column: "AirlineId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_AirportId",
                schema: "dbo",
                table: "Routes",
                column: "AirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_AirportId1",
                schema: "dbo",
                table: "Routes",
                column: "AirportId1");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DestinationAirportId",
                schema: "dbo",
                table: "Routes",
                column: "DestinationAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_OriginAirportId",
                schema: "dbo",
                table: "Routes",
                column: "OriginAirportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirplaneSeats",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FlightSeats",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Flights",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Airplanes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Routes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Airlines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Airports",
                schema: "dbo");
        }
    }
}
