using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSubAssetCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubAssetCategories",
                columns: table => new
                {
                    SUB_CAT_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MAIN_CAT_NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SUB_CAT_NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SUB_CAT_CODE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Creator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModifierId = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubAssetCategories", x => x.SUB_CAT_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubAssetCategories");
        }
    }
}
