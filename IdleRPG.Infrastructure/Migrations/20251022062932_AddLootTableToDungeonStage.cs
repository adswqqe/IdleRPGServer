using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLootTableToDungeonStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LootTableId",
                table: "DungeonStages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DungeonStages_LootTableId",
                table: "DungeonStages",
                column: "LootTableId");

            migrationBuilder.AddForeignKey(
                name: "FK_DungeonStages_LootTables_LootTableId",
                table: "DungeonStages",
                column: "LootTableId",
                principalTable: "LootTables",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DungeonStages_LootTables_LootTableId",
                table: "DungeonStages");

            migrationBuilder.DropIndex(
                name: "IX_DungeonStages_LootTableId",
                table: "DungeonStages");

            migrationBuilder.DropColumn(
                name: "LootTableId",
                table: "DungeonStages");
        }
    }
}
