using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleRPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttackSpeedToCharacterAndMonster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "AttackSpeed",
                table: "Monsters",
                type: "real",
                nullable: false,
                defaultValue: 1f);

            migrationBuilder.AddColumn<float>(
                name: "CritDamage",
                table: "Monsters",
                type: "real",
                nullable: false,
                defaultValue: 1.5f);

            migrationBuilder.AddColumn<float>(
                name: "CritRate",
                table: "Monsters",
                type: "real",
                nullable: false,
                defaultValue: 0.05f);

            migrationBuilder.AddColumn<float>(
                name: "Evasion",
                table: "Monsters",
                type: "real",
                nullable: false,
                defaultValue: 0.05f);

            migrationBuilder.AddColumn<float>(
                name: "AttackSpeed",
                table: "Characters",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.UpdateData(
                table: "Monsters",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "AttackSpeed", "CritDamage", "CritRate", "Evasion" },
                values: new object[] { 0.8f, 1.3f, 0.03f, 0.02f });

            migrationBuilder.UpdateData(
                table: "Monsters",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "AttackSpeed", "CritDamage", "CritRate", "Evasion" },
                values: new object[] { 1f, 1.5f, 0.05f, 0.05f });

            migrationBuilder.UpdateData(
                table: "Monsters",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "AttackSpeed", "CritDamage", "CritRate", "Evasion" },
                values: new object[] { 0.7f, 1.8f, 0.04f, 0.03f });

            migrationBuilder.UpdateData(
                table: "Monsters",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "AttackSpeed", "CritDamage", "CritRate", "Evasion" },
                values: new object[] { 0.6f, 2f, 0.03f, 0.02f });

            migrationBuilder.UpdateData(
                table: "Monsters",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "AttackSpeed", "CritDamage", "CritRate", "Evasion" },
                values: new object[] { 1.2f, 2f, 0.1f, 0.08f });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttackSpeed",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "CritDamage",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "CritRate",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "Evasion",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "AttackSpeed",
                table: "Characters");
        }
    }
}
