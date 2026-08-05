using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnePieceMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Translations",
                table: "Sagas",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Translations",
                table: "Islands",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Translations",
                table: "Events",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Translations",
                table: "CharacterVersions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Translations",
                table: "Arcs",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Translations",
                table: "Sagas");

            migrationBuilder.DropColumn(
                name: "Translations",
                table: "Islands");

            migrationBuilder.DropColumn(
                name: "Translations",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Translations",
                table: "CharacterVersions");

            migrationBuilder.DropColumn(
                name: "Translations",
                table: "Arcs");
        }
    }
}
