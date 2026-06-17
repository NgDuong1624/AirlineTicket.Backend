using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TRIGGER [flights].[TR_Airlines_SoftDelete]
ON [flights].[Airlines]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Airlines]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");

            migrationBuilder.Sql(@"
CREATE TRIGGER [flights].[TR_Airports_SoftDelete]
ON [flights].[Airports]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Airports]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");

            migrationBuilder.Sql(@"
CREATE TRIGGER [flights].[TR_Airplanes_SoftDelete]
ON [flights].[Airplanes]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Airplanes]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");

            migrationBuilder.Sql(@"
CREATE TRIGGER [flights].[TR_Routes_SoftDelete]
ON [flights].[Routes]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Routes]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");

            migrationBuilder.Sql(@"
CREATE TRIGGER [flights].[TR_Flights_SoftDelete]
ON [flights].[Flights]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Flights]
    SET [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [flights].[TR_Airlines_SoftDelete]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [flights].[TR_Airports_SoftDelete]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [flights].[TR_Airplanes_SoftDelete]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [flights].[TR_Routes_SoftDelete]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [flights].[TR_Flights_SoftDelete]");
        }
    }
}
