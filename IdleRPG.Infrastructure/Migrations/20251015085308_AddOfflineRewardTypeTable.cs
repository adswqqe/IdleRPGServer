using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfflineRewardTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OfflineRewardTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExperiencePerMinute = table.Column<int>(type: "integer", nullable: false),
                    GoldPerMinute = table.Column<int>(type: "integer", nullable: false),
                    MaxMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineRewardTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "OfflineRewardTypes",
                columns: new[] { "Id", "ExperiencePerMinute", "GoldPerMinute", "MaxMinutes", "Name" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 2, 1, 480, "Basic Offline Reward" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfflineRewardTypes");
        }
    }
}
