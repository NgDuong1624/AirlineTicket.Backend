using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Airplanes_AirplaneId1",
                schema: "dbo",
                table: "Flights");

            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Routes_RouteId1",
                schema: "dbo",
                table: "Flights");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airports_AirportId",
                schema: "dbo",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airports_AirportId1",
                schema: "dbo",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_AirportId",
                schema: "dbo",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_AirportId1",
                schema: "dbo",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Flights_AirplaneId1",
                schema: "dbo",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_RouteId1",
                schema: "dbo",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "AirportId",
                schema: "dbo",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "AirportId1",
                schema: "dbo",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "AirplaneId1",
                schema: "dbo",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "RouteId1",
                schema: "dbo",
                table: "Flights");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                schema: "dbo",
                table: "Airports",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                schema: "dbo",
                table: "Airports",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AirportId",
                schema: "dbo",
                table: "Routes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AirportId1",
                schema: "dbo",
                table: "Routes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AirplaneId1",
                schema: "dbo",
                table: "Flights",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RouteId1",
                schema: "dbo",
                table: "Flights",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                schema: "dbo",
                table: "Airports",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                schema: "dbo",
                table: "Airports",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

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
                name: "IX_Flights_AirplaneId1",
                schema: "dbo",
                table: "Flights",
                column: "AirplaneId1");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_RouteId1",
                schema: "dbo",
                table: "Flights",
                column: "RouteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Airplanes_AirplaneId1",
                schema: "dbo",
                table: "Flights",
                column: "AirplaneId1",
                principalSchema: "dbo",
                principalTable: "Airplanes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Routes_RouteId1",
                schema: "dbo",
                table: "Flights",
                column: "RouteId1",
                principalSchema: "dbo",
                principalTable: "Routes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airports_AirportId",
                schema: "dbo",
                table: "Routes",
                column: "AirportId",
                principalSchema: "dbo",
                principalTable: "Airports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airports_AirportId1",
                schema: "dbo",
                table: "Routes",
                column: "AirportId1",
                principalSchema: "dbo",
                principalTable: "Airports",
                principalColumn: "Id");
        }
    }
}
