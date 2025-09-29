using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalIdleTime",
                table: "PlayerStats");

            migrationBuilder.RenameColumn(
                name: "UpdateAt",
                table: "PlayerStats",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "OfflineHours",
                table: "PlayerStats",
                newName: "OfflineGoldMultiplier");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStage",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Players",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "TotalIdleTime",
                table: "Players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "MaxOfflineHours",
                table: "PlayerStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OfflineExpMultiplier",
                table: "PlayerStats",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VipLevel",
                table: "PlayerStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "RewardType",
                table: "OfflineRewards",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_PlayerId",
                table: "RefreshTokens",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "TotalIdleTime",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "MaxOfflineHours",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "OfflineExpMultiplier",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "VipLevel",
                table: "PlayerStats");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PlayerStats",
                newName: "UpdateAt");

            migrationBuilder.RenameColumn(
                name: "OfflineGoldMultiplier",
                table: "PlayerStats",
                newName: "OfflineHours");

            migrationBuilder.AddColumn<long>(
                name: "TotalIdleTime",
                table: "PlayerStats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<int>(
                name: "RewardType",
                table: "OfflineRewards",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
