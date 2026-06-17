using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Airplanes_Airlines_AirlineId",
                schema: "flights",
                table: "Airplanes");

            migrationBuilder.DropForeignKey(
                name: "FK_AirplaneSeats_Airplanes_AirplaneId",
                schema: "flights",
                table: "AirplaneSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_FlightSeats_Flights_FlightId",
                schema: "flights",
                table: "FlightSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airlines_AirlineId",
                schema: "flights",
                table: "Routes");

            migrationBuilder.AddForeignKey(
                name: "FK_Airplanes_Airlines_AirlineId",
                schema: "flights",
                table: "Airplanes",
                column: "AirlineId",
                principalSchema: "flights",
                principalTable: "Airlines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AirplaneSeats_Airplanes_AirplaneId",
                schema: "flights",
                table: "AirplaneSeats",
                column: "AirplaneId",
                principalSchema: "flights",
                principalTable: "Airplanes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FlightSeats_Flights_FlightId",
                schema: "flights",
                table: "FlightSeats",
                column: "FlightId",
                principalSchema: "flights",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airlines_AirlineId",
                schema: "flights",
                table: "Routes",
                column: "AirlineId",
                principalSchema: "flights",
                principalTable: "Airlines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Airplanes_Airlines_AirlineId",
                schema: "flights",
                table: "Airplanes");

            migrationBuilder.DropForeignKey(
                name: "FK_AirplaneSeats_Airplanes_AirplaneId",
                schema: "flights",
                table: "AirplaneSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_FlightSeats_Flights_FlightId",
                schema: "flights",
                table: "FlightSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airlines_AirlineId",
                schema: "flights",
                table: "Routes");

            migrationBuilder.AddForeignKey(
                name: "FK_Airplanes_Airlines_AirlineId",
                schema: "flights",
                table: "Airplanes",
                column: "AirlineId",
                principalSchema: "flights",
                principalTable: "Airlines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AirplaneSeats_Airplanes_AirplaneId",
                schema: "flights",
                table: "AirplaneSeats",
                column: "AirplaneId",
                principalSchema: "flights",
                principalTable: "Airplanes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlightSeats_Flights_FlightId",
                schema: "flights",
                table: "FlightSeats",
                column: "FlightId",
                principalSchema: "flights",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airlines_AirlineId",
                schema: "flights",
                table: "Routes",
                column: "AirlineId",
                principalSchema: "flights",
                principalTable: "Airlines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
