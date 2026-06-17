using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Users.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TRIGGER [users].[TR_Users_SoftDelete]
ON [users].[Users]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [users].[Users]
    SET [IsActive] = 0, [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [users].[TR_Users_SoftDelete]");
        }
    }
}
