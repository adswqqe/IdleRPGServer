using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDungeonSystemAndLootTablePattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DungeonTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinLevel = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MaxStackSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 999)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LootTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NumberOfRolls = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDungeonDailies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DungeonTemplateId = table.Column<int>(type: "integer", nullable: false),
                    DifficultyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDungeonDailies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDungeonDailies_DungeonTemplates_DungeonTemplateId",
                        column: x => x.DungeonTemplateId,
                        principalTable: "DungeonTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDungeonDailies_Players_UserId",
                        column: x => x.UserId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerItems_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerItems_ItemTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DungeonDifficulties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecommendedPower = table.Column<int>(type: "integer", nullable: false),
                    BaseGold = table.Column<long>(type: "bigint", nullable: false),
                    BaseExp = table.Column<int>(type: "integer", nullable: false),
                    LootTableId = table.Column<int>(type: "integer", nullable: true),
                    MaxWaves = table.Column<int>(type: "integer", nullable: false),
                    DailyEntryLimit = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    EntryCostGold = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonDifficulties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DungeonDifficulties_DungeonTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "DungeonTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DungeonDifficulties_LootTables_LootTableId",
                        column: x => x.LootTableId,
                        principalTable: "LootTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LootItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LootTableId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsGuaranteed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    MinQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    MaxQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LootItems_LootTables_LootTableId",
                        column: x => x.LootTableId,
                        principalTable: "LootTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DungeonProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    DifficultyId = table.Column<int>(type: "integer", nullable: false),
                    CurrentWave = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CurrentHealth = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DungeonProgresses_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DungeonProgresses_DungeonDifficulties_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "DungeonDifficulties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DungeonRunHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    DifficultyId = table.Column<int>(type: "integer", nullable: false),
                    IsCleared = table.Column<bool>(type: "boolean", nullable: false),
                    ClearedWave = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonRunHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DungeonRunHistories_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DungeonRunHistories_DungeonDifficulties_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "DungeonDifficulties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DungeonWaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DifficultyId = table.Column<int>(type: "integer", nullable: false),
                    WaveNumber = table.Column<int>(type: "integer", nullable: false),
                    MonsterId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonWaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DungeonWaves_DungeonDifficulties_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "DungeonDifficulties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DungeonWaves_Monsters_MonsterId",
                        column: x => x.MonsterId,
                        principalTable: "Monsters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DungeonDifficulties_LootTableId",
                table: "DungeonDifficulties",
                column: "LootTableId");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonDifficulties_TemplateId",
                table: "DungeonDifficulties",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonProgresses_CharacterId",
                table: "DungeonProgresses",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DungeonProgresses_DifficultyId",
                table: "DungeonProgresses",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonRunHistories_CharacterId_CompletedAt",
                table: "DungeonRunHistories",
                columns: new[] { "CharacterId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DungeonRunHistories_DifficultyId",
                table: "DungeonRunHistories",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonWaves_DifficultyId",
                table: "DungeonWaves",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_DungeonWaves_MonsterId",
                table: "DungeonWaves",
                column: "MonsterId");

            migrationBuilder.CreateIndex(
                name: "IX_LootItems_LootTableId",
                table: "LootItems",
                column: "LootTableId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerItems_CharacterId",
                table: "PlayerItems",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerItems_ItemTemplateId",
                table: "PlayerItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDungeonDailies_DungeonTemplateId",
                table: "UserDungeonDailies",
                column: "DungeonTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDungeonDailies_UserId_DungeonTemplateId_DifficultyCode_~",
                table: "UserDungeonDailies",
                columns: new[] { "UserId", "DungeonTemplateId", "DifficultyCode", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DungeonProgresses");

            migrationBuilder.DropTable(
                name: "DungeonRunHistories");

            migrationBuilder.DropTable(
                name: "DungeonWaves");

            migrationBuilder.DropTable(
                name: "LootItems");

            migrationBuilder.DropTable(
                name: "PlayerItems");

            migrationBuilder.DropTable(
                name: "UserDungeonDailies");

            migrationBuilder.DropTable(
                name: "DungeonDifficulties");

            migrationBuilder.DropTable(
                name: "ItemTemplates");

            migrationBuilder.DropTable(
                name: "DungeonTemplates");

            migrationBuilder.DropTable(
                name: "LootTables");
        }
    }
}
