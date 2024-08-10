using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiraSubathonTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaytimeCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaytimeStatisticsByTimestamps",
                columns: table => new
                {
                    IpAddress = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GroupingType = table.Column<int>(type: "int", nullable: false),
                    TotalMinutesPlayed = table.Column<double>(type: "float", nullable: false),
                    Cached = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaytimeStatisticsByTimestamps", x => new { x.IpAddress, x.Timestamp });
                    table.ForeignKey(
                        name: "FK_PlaytimeStatisticsByTimestamps_MinecraftServers_IpAddress",
                        column: x => x.IpAddress,
                        principalTable: "MinecraftServers",
                        principalColumn: "IpAddress",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaytimeStatisticsByTimestamps");
        }
    }
}
