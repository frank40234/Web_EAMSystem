using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class RenameSubCategoryMAINCODE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MAIN_CAT_NAME",
                table: "SubAssetCategories",
                newName: "MAIN_CAT_CODE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MAIN_CAT_CODE",
                table: "SubAssetCategories",
                newName: "MAIN_CAT_NAME");
        }
    }
}
