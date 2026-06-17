using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TRIGGER [bookings].[TR_Bookings_SoftDelete]
ON [bookings].[Bookings]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [bookings].[Bookings]
    SET [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [bookings].[TR_Bookings_SoftDelete]");
        }
    }
}
