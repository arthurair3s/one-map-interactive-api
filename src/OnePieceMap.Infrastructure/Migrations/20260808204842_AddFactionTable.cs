using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnePieceMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Faction",
                table: "CharacterVersions",
                newName: "FactionId");

            migrationBuilder.CreateTable(
                name: "Factions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Translations = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factions", x => x.Id);
                });

            // The RenameColumn above kept the existing 0-5 values (the old enum's underlying
            // ints) in place under FactionId — so the six rows below get those same explicit
            // ids, making every existing CharacterVersion resolve to the right row with no
            // separate backfill UPDATE needed. Order matches the old enum exactly:
            // StrawHatPirates, BuggyPirates, ArlongPirates, MarineForces, Unaffiliated,
            // BlackCatPirates.
            migrationBuilder.Sql(
                """
                INSERT INTO "Factions" ("Id", "Name", "Slug", "Translations") VALUES
                    (0, 'Straw Hat Pirates', 'straw-hat-pirates', '{"pt": {"Name": "Piratas do Chapéu de Palha"}}'),
                    (1, 'Buggy Pirates', 'buggy-pirates', '{"pt": {"Name": "Piratas do Buggy"}}'),
                    (2, 'Arlong Pirates', 'arlong-pirates', '{"pt": {"Name": "Piratas do Arlong"}}'),
                    (3, 'Marine Forces', 'marine-forces', '{"pt": {"Name": "Forças da Marinha"}}'),
                    (4, 'Unaffiliated', 'unaffiliated', '{"pt": {"Name": "Sem afiliação"}}'),
                    (5, 'Black Cat Pirates', 'black-cat-pirates', '{"pt": {"Name": "Piratas do Gato Negro"}}');

                SELECT setval(pg_get_serial_sequence('"Factions"', 'Id'), 5, true);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterVersions_FactionId",
                table: "CharacterVersions",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Factions_Name",
                table: "Factions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Factions_Slug",
                table: "Factions",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterVersions_Factions_FactionId",
                table: "CharacterVersions",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterVersions_Factions_FactionId",
                table: "CharacterVersions");

            migrationBuilder.DropTable(
                name: "Factions");

            migrationBuilder.DropIndex(
                name: "IX_CharacterVersions_FactionId",
                table: "CharacterVersions");

            migrationBuilder.RenameColumn(
                name: "FactionId",
                table: "CharacterVersions",
                newName: "Faction");
        }
    }
}
