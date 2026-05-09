using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperStorage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductNameAndUseStringCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                schema: "Wms",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "Wms",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Wms",
                table: "Products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                schema: "Wms",
                table: "Products",
                column: "Name");
        }
    }
}
