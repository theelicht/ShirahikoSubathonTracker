using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiraSubathonTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangePlayerPrimary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MinecraftPlayers",
                table: "MinecraftPlayers");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MinecraftPlayers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MinecraftPlayers",
                table: "MinecraftPlayers",
                columns: new[] { "IpAddress", "Uuid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MinecraftPlayers",
                table: "MinecraftPlayers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MinecraftPlayers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MinecraftPlayers",
                table: "MinecraftPlayers",
                columns: new[] { "IpAddress", "PlayerName" });
        }
    }
}
