using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBinIdToAssetInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BIN_ID",
                table: "AssetInfos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetInfos_BIN_ID",
                table: "AssetInfos",
                column: "BIN_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetInfos_StorageBins_BIN_ID",
                table: "AssetInfos",
                column: "BIN_ID",
                principalTable: "StorageBins",
                principalColumn: "BIN_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetInfos_StorageBins_BIN_ID",
                table: "AssetInfos");

            migrationBuilder.DropIndex(
                name: "IX_AssetInfos_BIN_ID",
                table: "AssetInfos");

            migrationBuilder.DropColumn(
                name: "BIN_ID",
                table: "AssetInfos");
        }
    }
}
