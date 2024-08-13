using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiraSubathonTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ModifyPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaytimeStatisticsByTimestamps",
                table: "PlaytimeStatisticsByTimestamps");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaytimeStatisticsByTimestamps",
                table: "PlaytimeStatisticsByTimestamps",
                columns: new[] { "IpAddress", "Timestamp", "GroupingType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaytimeStatisticsByTimestamps",
                table: "PlaytimeStatisticsByTimestamps");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaytimeStatisticsByTimestamps",
                table: "PlaytimeStatisticsByTimestamps",
                columns: new[] { "IpAddress", "Timestamp" });
        }
    }
}
