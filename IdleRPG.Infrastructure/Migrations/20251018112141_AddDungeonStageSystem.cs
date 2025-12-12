using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDungeonStageSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DungeonStageId",
                table: "BattleLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CharacterDungeonProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    HighestStageClearedNormal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HighestStageClearedHard = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HighestStageClearedNightmare = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterDungeonProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterDungeonProgresses_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DungeonStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequiredLevel = table.Column<int>(type: "integer", nullable: false),
                    MonsterId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseExperience = table.Column<int>(type: "integer", nullable: false),
                    BaseGold = table.Column<int>(type: "integer", nullable: false),
                    FirstClearBonusExp = table.Column<int>(type: "integer", nullable: true),
                    FirstClearBonusGold = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DungeonStages_Monsters_MonsterId",
                        column: x => x.MonsterId,
                        principalTable: "Monsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterDungeonProgresses_CharacterId_Unique",
                table: "CharacterDungeonProgresses",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DungeonStages_MonsterId",
                table: "DungeonStages",
                column: "MonsterId");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonStages_RequiredLevel",
                table: "DungeonStages",
                column: "RequiredLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterDungeonProgresses");

            migrationBuilder.DropTable(
                name: "DungeonStages");

            migrationBuilder.DropColumn(
                name: "DungeonStageId",
                table: "BattleLogs");
        }
    }
}
