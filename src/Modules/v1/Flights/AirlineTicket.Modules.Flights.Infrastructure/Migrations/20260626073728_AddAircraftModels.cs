using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeatClass",
                schema: "dbo",
                table: "AirplaneSeats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "AircraftModelId",
                schema: "dbo",
                table: "Airplanes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AircraftModels",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AircraftModelSeatTemplates",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AircraftModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatRow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatColumn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatClass = table.Column<int>(type: "int", nullable: false),
                    IsExtraLegroom = table.Column<bool>(type: "bit", nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftModelSeatTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AircraftModelSeatTemplates_AircraftModels_AircraftModelId",
                        column: x => x.AircraftModelId,
                        principalSchema: "dbo",
                        principalTable: "AircraftModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Airplanes_AircraftModelId",
                schema: "dbo",
                table: "Airplanes",
                column: "AircraftModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftModelSeatTemplates_AircraftModelId",
                schema: "dbo",
                table: "AircraftModelSeatTemplates",
                column: "AircraftModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Airplanes_AircraftModels_AircraftModelId",
                schema: "dbo",
                table: "Airplanes",
                column: "AircraftModelId",
                principalSchema: "dbo",
                principalTable: "AircraftModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Airplanes_AircraftModels_AircraftModelId",
                schema: "dbo",
                table: "Airplanes");

            migrationBuilder.DropTable(
                name: "AircraftModelSeatTemplates",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AircraftModels",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Airplanes_AircraftModelId",
                schema: "dbo",
                table: "Airplanes");

            migrationBuilder.DropColumn(
                name: "SeatClass",
                schema: "dbo",
                table: "AirplaneSeats");

            migrationBuilder.DropColumn(
                name: "AircraftModelId",
                schema: "dbo",
                table: "Airplanes");
        }
    }
}
