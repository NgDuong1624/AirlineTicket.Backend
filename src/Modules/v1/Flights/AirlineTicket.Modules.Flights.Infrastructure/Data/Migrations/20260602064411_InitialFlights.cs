using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialFlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "flights");

            migrationBuilder.CreateTable(
                name: "Airlines",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IataCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IataCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airplanes",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirlineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airplanes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Airplanes_Airlines_AirlineId",
                        column: x => x.AirlineId,
                        principalSchema: "flights",
                        principalTable: "Airlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirlineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginAirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationAirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirplaneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatClass = table.Column<int>(type: "int", nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                schema: "flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirplaneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ScheduledDeparture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledArrival = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualDeparture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatClass = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "Airlines",
                schema: "flights");

            migrationBuilder.DropTable(
                name: "Airports",
                schema: "flights");
        }
    }
}
