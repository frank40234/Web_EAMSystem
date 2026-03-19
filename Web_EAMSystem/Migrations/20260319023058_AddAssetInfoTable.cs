using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetInfos",
                columns: table => new
                {
                    ASSET_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ASSET_CODE = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    MAIN_CAT_CODE = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    SUB_CAT_CODE = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IN_CODE = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MODEL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SPEC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BRAND = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ASSET_UNIT_CODE = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Creator = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ModifierId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInfos", x => x.ASSET_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetInfos");
        }
    }
}
