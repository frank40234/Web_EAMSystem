using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoryForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MAIN_CAT_CODE",
                table: "SubAssetCategories");

            migrationBuilder.AddColumn<Guid>(
                name: "MAIN_CAT_ID",
                table: "SubAssetCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubAssetCategories_MAIN_CAT_ID",
                table: "SubAssetCategories",
                column: "MAIN_CAT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SubAssetCategories_AssetCategories_MAIN_CAT_ID",
                table: "SubAssetCategories",
                column: "MAIN_CAT_ID",
                principalTable: "AssetCategories",
                principalColumn: "MAIN_CAT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubAssetCategories_AssetCategories_MAIN_CAT_ID",
                table: "SubAssetCategories");

            migrationBuilder.DropIndex(
                name: "IX_SubAssetCategories_MAIN_CAT_ID",
                table: "SubAssetCategories");

            migrationBuilder.DropColumn(
                name: "MAIN_CAT_ID",
                table: "SubAssetCategories");

            migrationBuilder.AddColumn<string>(
                name: "MAIN_CAT_CODE",
                table: "SubAssetCategories",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }
    }
}
