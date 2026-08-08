using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnePieceMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIslandSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Islands",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Backfill before indexing: every existing row lands on the default ''
            // above, and a unique index over six identical empty strings cannot be
            // created. Derives the same slug shape the application validates for —
            // lowercase, non-alphanumerics collapsed to single hyphens, no edges.
            migrationBuilder.Sql(
                """
                UPDATE "Islands"
                SET "Slug" = trim(both '-' from regexp_replace(lower("Name"), '[^a-z0-9]+', '-', 'g'))
                WHERE "Slug" = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Islands_Slug",
                table: "Islands",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Islands_Slug",
                table: "Islands");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Islands");
        }
    }
}
