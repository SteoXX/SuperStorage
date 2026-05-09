using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperStorage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoriesAndAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                schema: "Wms",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Wms",
                table: "Products",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                schema: "Wms",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                schema: "Wms",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "Wms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                UPDATE "Wms"."Products"
                SET "Code" = "Sku",
                    "CreatedAtUtc" = NOW()
                WHERE "Code" IS NULL
                   OR "CreatedAtUtc" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Wms",
                table: "Products",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                schema: "Wms",
                table: "Products",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "Wms",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                schema: "Wms",
                table: "Products",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                schema: "Wms",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                schema: "Wms",
                table: "Products",
                column: "CategoryId",
                principalSchema: "Wms",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                schema: "Wms",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Wms");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                schema: "Wms",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Code",
                schema: "Wms",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "Wms",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Wms",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "Wms",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "Wms",
                table: "Products");
        }
    }
}
