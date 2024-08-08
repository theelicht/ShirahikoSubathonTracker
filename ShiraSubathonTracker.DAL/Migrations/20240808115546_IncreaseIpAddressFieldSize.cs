using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiraSubathonTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseIpAddressFieldSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_MinecraftPlayers_MinecraftServers_IpAddress", table: "MinecraftPlayers");
            
            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "MinecraftPlayers",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
            
            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "MinecraftServers",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
            
            migrationBuilder.AddForeignKey(
                name: "FK_MinecraftPlayers_MinecraftServers_IpAddress",
                table: "MinecraftPlayers",
                column: "IpAddress",
                principalTable: "MinecraftServers",
                principalColumn: "IpAddress",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_MinecraftPlayers_MinecraftServers_IpAddress", table: "MinecraftPlayers");
            
            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "MinecraftPlayers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);
            
            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "MinecraftServers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);
            
            migrationBuilder.AddForeignKey(
                name: "FK_MinecraftPlayers_MinecraftServers_IpAddress",
                table: "MinecraftPlayers",
                column: "IpAddress",
                principalTable: "MinecraftServers",
                principalColumn: "IpAddress",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
