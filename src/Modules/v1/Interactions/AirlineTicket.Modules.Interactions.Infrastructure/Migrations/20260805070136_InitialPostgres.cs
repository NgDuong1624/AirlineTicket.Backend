using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Interactions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "interactions");

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                schema: "interactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    airline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sender_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_connection_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    staff_connection_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                schema: "interactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    airline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    flight_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_verified_purchase = table.Column<bool>(type: "boolean", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reviews", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_airline_id_sent_at",
                schema: "interactions",
                table: "ChatMessages",
                columns: new[] { "airline_id", "sent_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_rating_created_at",
                schema: "interactions",
                table: "reviews",
                columns: new[] { "rating", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_user_id_created_at",
                schema: "interactions",
                table: "reviews",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_user_id_rating",
                schema: "interactions",
                table: "reviews",
                columns: new[] { "user_id", "rating" });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_user_id_rating_created_at",
                schema: "interactions",
                table: "reviews",
                columns: new[] { "user_id", "rating", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages",
                schema: "interactions");

            migrationBuilder.DropTable(
                name: "reviews",
                schema: "interactions");
        }
    }
}
