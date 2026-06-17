using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentUsages",
                schema: "promotions",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TargetAirlineId",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TargetFlightId",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.RenameColumn(
                name: "ValidTo",
                schema: "promotions",
                table: "Coupons",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "ValidFrom",
                schema: "promotions",
                table: "Coupons",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "MaxUsages",
                schema: "promotions",
                table: "Coupons",
                newName: "UsageCount");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "promotions",
                table: "Campaigns",
                newName: "IsFeatured");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxDiscountAmount",
                schema: "promotions",
                table: "Coupons",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "promotions",
                table: "Coupons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "promotions",
                table: "Coupons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderValue",
                schema: "promotions",
                table: "Coupons",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsageLimit",
                schema: "promotions",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                schema: "promotions",
                table: "Campaigns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                schema: "promotions",
                table: "Campaigns",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "promotions",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "promotions",
                table: "Campaigns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "promotions",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "promotions",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "MinOrderValue",
                schema: "promotions",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "UsageLimit",
                schema: "promotions",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Content",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "promotions",
                table: "Campaigns");

            migrationBuilder.RenameColumn(
                name: "UsageCount",
                schema: "promotions",
                table: "Coupons",
                newName: "MaxUsages");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "promotions",
                table: "Coupons",
                newName: "ValidTo");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                schema: "promotions",
                table: "Coupons",
                newName: "ValidFrom");

            migrationBuilder.RenameColumn(
                name: "IsFeatured",
                schema: "promotions",
                table: "Campaigns",
                newName: "IsActive");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxDiscountAmount",
                schema: "promotions",
                table: "Coupons",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentUsages",
                schema: "promotions",
                table: "Coupons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "promotions",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DiscountType",
                schema: "promotions",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                schema: "promotions",
                table: "Campaigns",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "promotions",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetAirlineId",
                schema: "promotions",
                table: "Campaigns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetFlightId",
                schema: "promotions",
                table: "Campaigns",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
