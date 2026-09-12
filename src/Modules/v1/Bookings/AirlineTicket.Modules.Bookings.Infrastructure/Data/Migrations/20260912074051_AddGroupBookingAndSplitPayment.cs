using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupBookingAndSplitPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "group_bookings",
                schema: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    leader_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    return_flight_id = table.Column<Guid>(type: "uuid", nullable: true),
                    group_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    invite_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    split_strategy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_bookings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "group_members",
                schema: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    passenger_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    passenger_email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    passenger_phone = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    seat_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    return_seat_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    assigned_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    payment_transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payment_provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_members_group_bookings_group_booking_id",
                        column: x => x.group_booking_id,
                        principalSchema: "bookings",
                        principalTable: "group_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_group_bookings_invite_code",
                schema: "bookings",
                table: "group_bookings",
                column: "invite_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_bookings_status_expires_at",
                schema: "bookings",
                table: "group_bookings",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_group_members_group_booking_id",
                schema: "bookings",
                table: "group_members",
                column: "group_booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_members_passenger_email",
                schema: "bookings",
                table: "group_members",
                column: "passenger_email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_members",
                schema: "bookings");

            migrationBuilder.DropTable(
                name: "group_bookings",
                schema: "bookings");
        }
    }
}
