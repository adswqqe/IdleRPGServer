using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillGachaSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Crystals",
                table: "Players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Crystal",
                table: "Characters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "GachaPityCount",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SkillTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillTemplateId = table.Column<int>(type: "integer", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterSkills_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterSkills_SkillTemplates_SkillTemplateId",
                        column: x => x.SkillTemplateId,
                        principalTable: "SkillTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GachaHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillTemplateId = table.Column<int>(type: "integer", nullable: false),
                    WasPity = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GachaHistories_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GachaHistories_SkillTemplates_SkillTemplateId",
                        column: x => x.SkillTemplateId,
                        principalTable: "SkillTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkills_CharacterId_SkillTemplateId",
                table: "CharacterSkills",
                columns: new[] { "CharacterId", "SkillTemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkills_SkillTemplateId",
                table: "CharacterSkills",
                column: "SkillTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_GachaHistories_CharacterId_CreatedAt",
                table: "GachaHistories",
                columns: new[] { "CharacterId", "CreatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_GachaHistories_SkillTemplateId",
                table: "GachaHistories",
                column: "SkillTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillTemplates_Rarity",
                table: "SkillTemplates",
                column: "Rarity");

            migrationBuilder.CreateIndex(
                name: "IX_SkillTemplates_Type",
                table: "SkillTemplates",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterSkills");

            migrationBuilder.DropTable(
                name: "GachaHistories");

            migrationBuilder.DropTable(
                name: "SkillTemplates");

            migrationBuilder.DropColumn(
                name: "Crystals",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Crystal",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "GachaPityCount",
                table: "Characters");
        }
    }
}
