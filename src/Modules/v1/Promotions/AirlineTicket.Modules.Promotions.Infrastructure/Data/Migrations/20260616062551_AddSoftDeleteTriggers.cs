using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TRIGGER [promotions].[TR_Coupons_SoftDelete]
ON [promotions].[Coupons]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [promotions].[Coupons]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");

            migrationBuilder.Sql(@"
CREATE TRIGGER [promotions].[TR_Campaigns_SoftDelete]
ON [promotions].[Campaigns]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [promotions].[Campaigns]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [promotions].[TR_Coupons_SoftDelete]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [promotions].[TR_Campaigns_SoftDelete]");
        }
    }
}
