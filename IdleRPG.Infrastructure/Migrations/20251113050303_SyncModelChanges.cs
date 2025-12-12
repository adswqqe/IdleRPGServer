using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterDungeonProgresses");

            migrationBuilder.DropColumn(
                name: "Crystals",
                table: "Players");

            migrationBuilder.AddColumn<int>(
                name: "PetGachaCount",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CharacterBattleProgresses",
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
                    table.PrimaryKey("PK_CharacterBattleProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterBattleProgresses_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GuildId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PetTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    BaseAttack = table.Column<int>(type: "integer", nullable: false),
                    BaseMana = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetTemplates", x => x.Id);
                    table.CheckConstraint("CK_PetTemplates_BaseAttack", "\"BaseAttack\" >= 0");
                    table.CheckConstraint("CK_PetTemplates_BaseMana", "\"BaseMana\" >= 0");
                    table.CheckConstraint("CK_PetTemplates_Rarity", "\"Rarity\" >= 0 AND \"Rarity\" <= 3");
                });

            migrationBuilder.CreateTable(
                name: "PvpSeason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvpSeason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Characters_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRoomParticipants_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoomParticipants_ChatRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CurrentAttack = table.Column<int>(type: "integer", nullable: false),
                    CurrentMana = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.CheckConstraint("CK_Pets_CurrentAttack", "\"CurrentAttack\" >= 0");
                    table.CheckConstraint("CK_Pets_CurrentMana", "\"CurrentMana\" >= 0");
                    table.CheckConstraint("CK_Pets_Level", "\"Level\" >= 1 AND \"Level\" <= 50");
                    table.ForeignKey(
                        name: "FK_Pets_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pets_PetTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "PetTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PvpMatch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    AttackerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttackerRatingBefore = table.Column<int>(type: "integer", nullable: false),
                    AttackerRatingAfter = table.Column<int>(type: "integer", nullable: false),
                    DefenderRatingBefore = table.Column<int>(type: "integer", nullable: false),
                    DefenderRatingAfter = table.Column<int>(type: "integer", nullable: false),
                    SeasonId1 = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvpMatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PvpMatch_Characters_AttackerId",
                        column: x => x.AttackerId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PvpMatch_Characters_DefenderId",
                        column: x => x.DefenderId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PvpMatch_Characters_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PvpMatch_PvpSeason_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "PvpSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PvpMatch_PvpSeason_SeasonId1",
                        column: x => x.SeasonId1,
                        principalTable: "PvpSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PvpRanking",
                columns: table => new
                {
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    WinStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsRewardClaimed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastMatchAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SeasonId1 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvpRanking", x => new { x.SeasonId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_PvpRanking_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PvpRanking_PvpSeason_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "PvpSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PvpRanking_PvpSeason_SeasonId1",
                        column: x => x.SeasonId1,
                        principalTable: "PvpSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquippedPets",
                columns: table => new
                {
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotIndex = table.Column<int>(type: "integer", nullable: false),
                    PetId = table.Column<int>(type: "integer", nullable: false),
                    EquippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquippedPets", x => new { x.CharacterId, x.SlotIndex });
                    table.CheckConstraint("CK_EquippedPets_SlotIndex", "\"SlotIndex\" >= 1 AND \"SlotIndex\" <= 3");
                    table.ForeignKey(
                        name: "FK_EquippedPets_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquippedPets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterBattleProgresses_CharacterId_Unique",
                table: "CharacterBattleProgresses",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_RoomId_CreatedAt",
                table: "ChatMessages",
                columns: new[] { "RoomId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_CharacterId",
                table: "ChatRoomParticipants",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_RoomId",
                table: "ChatRoomParticipants",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_RoomId_CharacterId",
                table: "ChatRoomParticipants",
                columns: new[] { "RoomId", "CharacterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_GuildId",
                table: "ChatRooms",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_Type",
                table: "ChatRooms",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "UK_EquippedPets_PetId",
                table: "EquippedPets",
                column: "PetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_CharacterId",
                table: "Pets",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_TemplateId",
                table: "Pets",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PetTemplates_Rarity",
                table: "PetTemplates",
                column: "Rarity");

            migrationBuilder.CreateIndex(
                name: "UK_PetTemplates_Name",
                table: "PetTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PvpMatch_AttackerId_CreatedAt",
                table: "PvpMatch",
                columns: new[] { "AttackerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PvpMatch_DefenderId_CreatedAt",
                table: "PvpMatch",
                columns: new[] { "DefenderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PvpMatch_SeasonId",
                table: "PvpMatch",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PvpMatch_SeasonId1",
                table: "PvpMatch",
                column: "SeasonId1");

            migrationBuilder.CreateIndex(
                name: "IX_PvpMatch_WinnerId",
                table: "PvpMatch",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PvpRanking_CharacterId",
                table: "PvpRanking",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_PvpRanking_SeasonId_Rating_DESC",
                table: "PvpRanking",
                columns: new[] { "SeasonId", "Rating" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PvpRanking_SeasonId1",
                table: "PvpRanking",
                column: "SeasonId1");

            migrationBuilder.CreateIndex(
                name: "IX_PvpSeason_IsActive",
                table: "PvpSeason",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PvpSeason_SeasonNumber",
                table: "PvpSeason",
                column: "SeasonNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterBattleProgresses");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatRoomParticipants");

            migrationBuilder.DropTable(
                name: "EquippedPets");

            migrationBuilder.DropTable(
                name: "PvpMatch");

            migrationBuilder.DropTable(
                name: "PvpRanking");

            migrationBuilder.DropTable(
                name: "ChatRooms");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "PvpSeason");

            migrationBuilder.DropTable(
                name: "PetTemplates");

            migrationBuilder.DropColumn(
                name: "PetGachaCount",
                table: "Characters");

            migrationBuilder.AddColumn<long>(
                name: "Crystals",
                table: "Players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "CharacterDungeonProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HighestStageClearedHard = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HighestStageClearedNightmare = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HighestStageClearedNormal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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

            migrationBuilder.CreateIndex(
                name: "IX_CharacterDungeonProgresses_CharacterId_Unique",
                table: "CharacterDungeonProgresses",
                column: "CharacterId",
                unique: true);
        }
    }
}
