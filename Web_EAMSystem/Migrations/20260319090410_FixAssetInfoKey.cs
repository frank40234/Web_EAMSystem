using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixAssetInfoKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ASSET_UNIT_CODE",
                table: "AssetInfos");

            migrationBuilder.DropColumn(
                name: "IN",
                table: "AssetInfos");

            migrationBuilder.DropColumn(
                name: "IN_CODE",
                table: "AssetInfos");

            migrationBuilder.DropColumn(
                name: "MAIN_CAT_CODE",
                table: "AssetInfos");

            migrationBuilder.DropColumn(
                name: "SPEC",
                table: "AssetInfos");

            migrationBuilder.DropColumn(
                name: "SUB_CAT_CODE",
                table: "AssetInfos");

            migrationBuilder.AlterColumn<string>(
                name: "ModifierId",
                table: "AssetInfos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Modifier",
                table: "AssetInfos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "AssetInfos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "AssetInfos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "ASSET_CODE",
                table: "AssetInfos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);

            migrationBuilder.AddColumn<Guid>(
                name: "IN_ID",
                table: "AssetInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UNIT_ID",
                table: "AssetInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AssetInfos_IN_ID",
                table: "AssetInfos",
                column: "IN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInfos_UNIT_ID",
                table: "AssetInfos",
                column: "UNIT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetInfos_AssetUnits_UNIT_ID",
                table: "AssetInfos",
                column: "UNIT_ID",
                principalTable: "AssetUnits",
                principalColumn: "ASSET_UNIT_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetInfos_ItemNames_IN_ID",
                table: "AssetInfos",
                column: "IN_ID",
                principalTable: "ItemNames",
                principalColumn: "IN_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetInfos_AssetUnits_UNIT_ID",
                table: "AssetInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetInfos_ItemNames_IN_ID",
                table: "AssetInfos");

            migrationBuilder.DropIndex(
                name: "IX_AssetInfos_IN_ID",
                table: "AssetInfos");

            migrationBuilder.DropIndex(
                name: "IX_AssetInfos_UNIT_ID",
                table: "AssetInfos");

            migrationBuilder.DropColumn(
                name: "IN_ID",
                table: "AssetInfos");

            migrationBuilder.DropColumn(
                name: "UNIT_ID",
                table: "AssetInfos");

            migrationBuilder.AlterColumn<string>(
                name: "ModifierId",
                table: "AssetInfos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modifier",
                table: "AssetInfos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "AssetInfos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "AssetInfos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ASSET_CODE",
                table: "AssetInfos",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ASSET_UNIT_CODE",
                table: "AssetInfos",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IN",
                table: "AssetInfos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IN_CODE",
                table: "AssetInfos",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MAIN_CAT_CODE",
                table: "AssetInfos",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SPEC",
                table: "AssetInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SUB_CAT_CODE",
                table: "AssetInfos",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }
    }
}
