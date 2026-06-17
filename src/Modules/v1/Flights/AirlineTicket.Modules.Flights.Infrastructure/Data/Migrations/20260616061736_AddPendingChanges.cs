using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                schema: "flights",
                table: "FlightSeats");

            migrationBuilder.DropColumn(
                name: "ActualArrival",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ActualDeparture",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "SeatClass",
                schema: "flights",
                table: "AirplaneSeats");

            migrationBuilder.RenameColumn(
                name: "ScheduledDeparture",
                schema: "flights",
                table: "Flights",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ScheduledArrival",
                schema: "flights",
                table: "Flights",
                newName: "DepartureTime");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "flights",
                table: "Airports",
                newName: "NameVi");

            migrationBuilder.RenameColumn(
                name: "Country",
                schema: "flights",
                table: "Airports",
                newName: "CityVi");

            migrationBuilder.RenameColumn(
                name: "City",
                schema: "flights",
                table: "Airports",
                newName: "CityEn");

            migrationBuilder.AddColumn<Guid>(
                name: "AirportId",
                schema: "flights",
                table: "Routes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AirportId1",
                schema: "flights",
                table: "Routes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DistanceKm",
                schema: "flights",
                table: "Routes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                schema: "flights",
                table: "Routes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "flights",
                table: "Routes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExtraLegroom",
                schema: "flights",
                table: "FlightSeats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceOverride",
                schema: "flights",
                table: "FlightSeats",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AirplaneId1",
                schema: "flights",
                table: "Flights",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalTime",
                schema: "flights",
                table: "Flights",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "flights",
                table: "Flights",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "flights",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "flights",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "flights",
                table: "Flights",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RouteId1",
                schema: "flights",
                table: "Flights",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                schema: "flights",
                table: "Airports",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "flights",
                table: "Airports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "flights",
                table: "Airports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                schema: "flights",
                table: "Airports",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                schema: "flights",
                table: "Airports",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                schema: "flights",
                table: "Airports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsExtraLegroom",
                schema: "flights",
                table: "AirplaneSeats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SeatColumn",
                schema: "flights",
                table: "AirplaneSeats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatRow",
                schema: "flights",
                table: "AirplaneSeats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "flights",
                table: "Airplanes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApiEndpoint",
                schema: "flights",
                table: "Airlines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                schema: "flights",
                table: "Airlines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseCountry",
                schema: "flights",
                table: "Airlines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "flights",
                table: "Airlines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "flights",
                table: "Airlines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "flights",
                table: "Airlines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                schema: "flights",
                table: "Airlines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_AirportId",
                schema: "flights",
                table: "Routes",
                column: "AirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_AirportId1",
                schema: "flights",
                table: "Routes",
                column: "AirportId1");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AirplaneId1",
                schema: "flights",
                table: "Flights",
                column: "AirplaneId1");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_RouteId1",
                schema: "flights",
                table: "Flights",
                column: "RouteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Airplanes_AirplaneId1",
                schema: "flights",
                table: "Flights",
                column: "AirplaneId1",
                principalSchema: "flights",
                principalTable: "Airplanes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Routes_RouteId1",
                schema: "flights",
                table: "Flights",
                column: "RouteId1",
                principalSchema: "flights",
                principalTable: "Routes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airports_AirportId",
                schema: "flights",
                table: "Routes",
                column: "AirportId",
                principalSchema: "flights",
                principalTable: "Airports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airports_AirportId1",
                schema: "flights",
                table: "Routes",
                column: "AirportId1",
                principalSchema: "flights",
                principalTable: "Airports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Airplanes_AirplaneId1",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Routes_RouteId1",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airports_AirportId",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airports_AirportId1",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_AirportId",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_AirportId1",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Flights_AirplaneId1",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_RouteId1",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "AirportId",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "AirportId1",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DistanceKm",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "flights",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "IsExtraLegroom",
                schema: "flights",
                table: "FlightSeats");

            migrationBuilder.DropColumn(
                name: "PriceOverride",
                schema: "flights",
                table: "FlightSeats");

            migrationBuilder.DropColumn(
                name: "AirplaneId1",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalTime",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "RouteId1",
                schema: "flights",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                schema: "flights",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "flights",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "flights",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "flights",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "flights",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "NameEn",
                schema: "flights",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "IsExtraLegroom",
                schema: "flights",
                table: "AirplaneSeats");

            migrationBuilder.DropColumn(
                name: "SeatColumn",
                schema: "flights",
                table: "AirplaneSeats");

            migrationBuilder.DropColumn(
                name: "SeatRow",
                schema: "flights",
                table: "AirplaneSeats");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "flights",
                table: "Airplanes");

            migrationBuilder.DropColumn(
                name: "ApiEndpoint",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.DropColumn(
                name: "BaseCountry",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                schema: "flights",
                table: "Airlines");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "flights",
                table: "Flights",
                newName: "ScheduledDeparture");

            migrationBuilder.RenameColumn(
                name: "DepartureTime",
                schema: "flights",
                table: "Flights",
                newName: "ScheduledArrival");

            migrationBuilder.RenameColumn(
                name: "NameVi",
                schema: "flights",
                table: "Airports",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CityVi",
                schema: "flights",
                table: "Airports",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "CityEn",
                schema: "flights",
                table: "Airports",
                newName: "City");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                schema: "flights",
                table: "FlightSeats",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualArrival",
                schema: "flights",
                table: "Flights",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualDeparture",
                schema: "flights",
                table: "Flights",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeatClass",
                schema: "flights",
                table: "AirplaneSeats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
