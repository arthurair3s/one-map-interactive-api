using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnePieceMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFactionToCharacterVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Faction",
                table: "CharacterVersions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Faction",
                table: "CharacterVersions");
        }
    }
}
