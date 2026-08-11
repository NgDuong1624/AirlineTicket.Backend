using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.CMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCMSModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aircraft_model",
                schema: "cms",
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
                    table.PrimaryKey("pk_aircraft_model", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "airplane_seat",
                schema: "cms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    airplane_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_number = table.Column<string>(type: "text", nullable: false),
                    seat_row = table.Column<string>(type: "text", nullable: false),
                    seat_column = table.Column<string>(type: "text", nullable: false),
                    seat_class = table.Column<int>(type: "integer", nullable: false),
                    is_extra_legroom = table.Column<bool>(type: "boolean", nullable: false),
                    price_multiplier = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_airplane_seat", x => x.id);
                    table.ForeignKey(
                        name: "fk_airplane_seat_airplanes_airplane_id",
                        column: x => x.airplane_id,
                        principalSchema: "flights",
                        principalTable: "airplanes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aircraft_model_seat_template",
                schema: "cms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aircraft_model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_number = table.Column<string>(type: "text", nullable: false),
                    seat_row = table.Column<string>(type: "text", nullable: false),
                    seat_column = table.Column<string>(type: "text", nullable: false),
                    seat_class = table.Column<int>(type: "integer", nullable: false),
                    is_extra_legroom = table.Column<bool>(type: "boolean", nullable: false),
                    price_multiplier = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aircraft_model_seat_template", x => x.id);
                    table.ForeignKey(
                        name: "fk_aircraft_model_seat_template_aircraft_model_aircraft_model_",
                        column: x => x.aircraft_model_id,
                        principalSchema: "cms",
                        principalTable: "aircraft_model",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aircraft_model_seat_template_aircraft_model_id",
                schema: "cms",
                table: "aircraft_model_seat_template",
                column: "aircraft_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_airplane_seat_airplane_id",
                schema: "cms",
                table: "airplane_seat",
                column: "airplane_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft_model_seat_template",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "airplane_seat",
                schema: "cms");

            migrationBuilder.DropTable(
                name: "aircraft_model",
                schema: "cms");
        }
    }
}
