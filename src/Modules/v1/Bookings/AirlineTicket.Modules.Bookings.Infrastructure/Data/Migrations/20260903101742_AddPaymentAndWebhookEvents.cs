using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAndWebhookEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_payments_booking_id_created_at",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_booking_id_provider_status",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_booking_id_provider_status_created_at",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_provider_status_created_at",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "is_successful",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payment_method",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider_status",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "transaction_id",
                schema: "bookings",
                table: "payments");

            migrationBuilder.AddColumn<int>(
                name: "concurrency_version",
                schema: "bookings",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "currency",
                schema: "bookings",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                schema: "bookings",
                table: "payments",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "provider",
                schema: "bookings",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "provider_timestamp",
                schema: "bookings",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_transaction_id",
                schema: "bookings",
                table: "payments",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "status",
                schema: "bookings",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "bookings",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "webhook_events",
                schema: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<int>(type: "integer", nullable: false),
                    provider_event_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id",
                schema: "bookings",
                table: "payments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id_status_created_at",
                schema: "bookings",
                table: "payments",
                columns: new[] { "booking_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_provider_provider_transaction_id",
                schema: "bookings",
                table: "payments",
                columns: new[] { "provider", "provider_transaction_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_status_created_at",
                schema: "bookings",
                table: "payments",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_provider_provider_event_id",
                schema: "bookings",
                table: "webhook_events",
                columns: new[] { "provider", "provider_event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_status_received_at",
                schema: "bookings",
                table: "webhook_events",
                columns: new[] { "status", "received_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhook_events",
                schema: "bookings");

            migrationBuilder.DropIndex(
                name: "ix_payments_booking_id",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_booking_id_status_created_at",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_provider_provider_transaction_id",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_status_created_at",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "concurrency_version",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "currency",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "failure_reason",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider_timestamp",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider_transaction_id",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "bookings",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "bookings",
                table: "payments");

            migrationBuilder.AddColumn<bool>(
                name: "is_successful",
                schema: "bookings",
                table: "payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                schema: "bookings",
                table: "payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "provider_status",
                schema: "bookings",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_id",
                schema: "bookings",
                table: "payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id_created_at",
                schema: "bookings",
                table: "payments",
                columns: new[] { "booking_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id_provider_status",
                schema: "bookings",
                table: "payments",
                columns: new[] { "booking_id", "provider_status" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id_provider_status_created_at",
                schema: "bookings",
                table: "payments",
                columns: new[] { "booking_id", "provider_status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_provider_status_created_at",
                schema: "bookings",
                table: "payments",
                columns: new[] { "provider_status", "created_at" });
        }
    }
}
