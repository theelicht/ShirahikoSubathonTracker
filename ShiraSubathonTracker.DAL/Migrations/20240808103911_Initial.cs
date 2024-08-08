using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiraSubathonTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinecraftVersions",
                columns: table => new
                {
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServerProtocol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinecraftVersions", x => x.Version);
                });

            migrationBuilder.CreateTable(
                name: "MinecraftServers",
                columns: table => new
                {
                    IpAddress = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DnsName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MessageOfTheDay = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ServerStatus = table.Column<int>(type: "int", nullable: false),
                    LastSeenOnline = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CurrentServer = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinecraftServers", x => x.IpAddress);
                    table.ForeignKey(
                        name: "FK_MinecraftServers_MinecraftVersions_Version",
                        column: x => x.Version,
                        principalTable: "MinecraftVersions",
                        principalColumn: "Version",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MinecraftPlayers",
                columns: table => new
                {
                    IpAddress = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlayerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Uuid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SecondsOnline = table.Column<long>(type: "bigint", nullable: false),
                    LastSeenOnline = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinecraftPlayers", x => new { x.IpAddress, x.PlayerName });
                    table.ForeignKey(
                        name: "FK_MinecraftPlayers_MinecraftServers_IpAddress",
                        column: x => x.IpAddress,
                        principalTable: "MinecraftServers",
                        principalColumn: "IpAddress",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MinecraftServers_Version",
                table: "MinecraftServers",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinecraftPlayers");

            migrationBuilder.DropTable(
                name: "MinecraftServers");

            migrationBuilder.DropTable(
                name: "MinecraftVersions");
        }
    }
}
