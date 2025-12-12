using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillRewardSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemTemplateId",
                table: "LootItems",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemTemplateId",
                table: "LootItems");
        }
    }
}
