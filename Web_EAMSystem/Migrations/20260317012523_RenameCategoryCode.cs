using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "AssetCategories",
                newName: "MAIN_CAT_NAME");

            migrationBuilder.RenameColumn(
                name: "CategoryCode",
                table: "AssetCategories",
                newName: "MAIN_CAT_CODE");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "AssetCategories",
                newName: "MAIN_CAT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MAIN_CAT_NAME",
                table: "AssetCategories",
                newName: "CategoryName");

            migrationBuilder.RenameColumn(
                name: "MAIN_CAT_CODE",
                table: "AssetCategories",
                newName: "CategoryCode");

            migrationBuilder.RenameColumn(
                name: "MAIN_CAT_ID",
                table: "AssetCategories",
                newName: "CategoryId");
        }
    }
}
