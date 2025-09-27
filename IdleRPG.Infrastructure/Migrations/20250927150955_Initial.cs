using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    MaxStackSize = table.Column<int>(type: "integer", nullable: false),
                    SellPrice = table.Column<long>(type: "bigint", nullable: false),
                    BaseStats = table.Column<string>(type: "text", nullable: false),
                    CanEnhance = table.Column<bool>(type: "boolean", nullable: false),
                    IsTradeable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CharacterClass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Experience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Health = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    Mana = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    Attack = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    Defense = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Experience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Gold = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1000L),
                    Gems = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OfflineHours = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalIdleTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_PlayerStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfflineRewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfflineStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    OfflineEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveOfflineMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RewardType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ExperienceGained = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    GoldGained = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    RewardItems = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    RewardMultiplier = table.Column<float>(type: "real", precision: 3, scale: 2, nullable: false, defaultValue: 1f),
                    IsClaimed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfflineRewards_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerInventories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: -1),
                    ItemTemplateId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    EnhancementLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AcquiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AdditionalOptions = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerInventories_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerInventories_ItemTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ItemTemplates",
                columns: new[] { "Id", "BaseStats", "CanEnhance", "Description", "IsTradeable", "MaxStackSize", "Name", "Rarity", "SellPrice", "Type" },
                values: new object[,]
                {
                    { 1001, "{\"attack\": 5, \"durability\": 100}", false, "초보자용 나무 검", true, 1, "나무 검", 1, 10L, 0 },
                    { 1002, "{\"healAmount\": 50}", false, "HP를 50 회복한다", true, 99, "체력 포션", 1, 5L, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_PlayerId",
                table: "Characters",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_PlayerId_IsMain",
                table: "Characters",
                columns: new[] { "PlayerId", "IsMain" });

            migrationBuilder.CreateIndex(
                name: "IX_OfflineRewards_CharacterId",
                table: "OfflineRewards",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineRewards_CharacterId_IsClaimed",
                table: "OfflineRewards",
                columns: new[] { "CharacterId", "IsClaimed" });

            migrationBuilder.CreateIndex(
                name: "IX_OfflineRewards_IsClaimed",
                table: "OfflineRewards",
                column: "IsClaimed");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventories_CharacterId",
                table: "PlayerInventories",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventories_CharacterId_SlotIndex",
                table: "PlayerInventories",
                columns: new[] { "CharacterId", "SlotIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventories_ExpiresAt",
                table: "PlayerInventories",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventories_ItemTemplateId",
                table: "PlayerInventories",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                table: "Players",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_UserName",
                table: "Players",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfflineRewards");

            migrationBuilder.DropTable(
                name: "PlayerInventories");

            migrationBuilder.DropTable(
                name: "PlayerStats");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "ItemTemplates");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
