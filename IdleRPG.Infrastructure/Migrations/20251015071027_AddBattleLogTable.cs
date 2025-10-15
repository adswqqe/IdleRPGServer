using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBattleLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BattleLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonsterId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsVictory = table.Column<bool>(type: "boolean", nullable: false),
                    ExperienceGained = table.Column<int>(type: "integer", nullable: false),
                    GoldGained = table.Column<int>(type: "integer", nullable: false),
                    DamageDealt = table.Column<int>(type: "integer", nullable: false),
                    DamageTaken = table.Column<int>(type: "integer", nullable: false),
                    BattleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleLogs_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BattleLogs_Monsters_MonsterId",
                        column: x => x.MonsterId,
                        principalTable: "Monsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BattleLogs_CharacterId_BattleDate",
                table: "BattleLogs",
                columns: new[] { "CharacterId", "BattleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BattleLogs_MonsterId",
                table: "BattleLogs",
                column: "MonsterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattleLogs");
        }
    }
}
