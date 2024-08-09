using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiraSubathonTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSubathonEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subathons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsCurrentSubathon = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subathons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubGifts",
                columns: table => new
                {
                    TwitchUsername = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfGift = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AmountGifted = table.Column<int>(type: "int", nullable: false),
                    SubathonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubGifts", x => new { x.TwitchUsername, x.DateOfGift });
                    table.ForeignKey(
                        name: "FK_SubGifts_Subathons_SubathonId",
                        column: x => x.SubathonId,
                        principalTable: "Subathons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubGifts_SubathonId",
                table: "SubGifts",
                column: "SubathonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubGifts");

            migrationBuilder.DropTable(
                name: "Subathons");
        }
    }
}
