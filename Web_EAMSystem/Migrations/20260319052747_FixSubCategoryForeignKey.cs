using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixSubCategoryForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SUB_CAT_CODE",
                table: "ItemNames");

            migrationBuilder.AddColumn<Guid>(
                name: "SUB_CAT_ID",
                table: "ItemNames",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemNames_SUB_CAT_ID",
                table: "ItemNames",
                column: "SUB_CAT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemNames_SubAssetCategories_SUB_CAT_ID",
                table: "ItemNames",
                column: "SUB_CAT_ID",
                principalTable: "SubAssetCategories",
                principalColumn: "SUB_CAT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemNames_SubAssetCategories_SUB_CAT_ID",
                table: "ItemNames");

            migrationBuilder.DropIndex(
                name: "IX_ItemNames_SUB_CAT_ID",
                table: "ItemNames");

            migrationBuilder.DropColumn(
                name: "SUB_CAT_ID",
                table: "ItemNames");

            migrationBuilder.AddColumn<string>(
                name: "SUB_CAT_CODE",
                table: "ItemNames",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }
    }
}
