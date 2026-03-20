using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_EAMSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreRoomAndBin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoreRooms",
                columns: table => new
                {
                    ROOM_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ROOM_NAME = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
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
                    table.PrimaryKey("PK_StoreRooms", x => x.ROOM_ID);
                });

            migrationBuilder.CreateTable(
                name: "StorageBins",
                columns: table => new
                {
                    BIN_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ROOM_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BIN_CODE = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Creator = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatorId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ModifierId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageBins", x => x.BIN_ID);
                    table.ForeignKey(
                        name: "FK_StorageBins_StoreRooms_ROOM_ID",
                        column: x => x.ROOM_ID,
                        principalTable: "StoreRooms",
                        principalColumn: "ROOM_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorageBins_ROOM_ID",
                table: "StorageBins",
                column: "ROOM_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorageBins");

            migrationBuilder.DropTable(
                name: "StoreRooms");
        }
    }
}
