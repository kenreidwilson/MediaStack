using Microsoft.EntityFrameworkCore.Migrations;

namespace MediaStack_Library.Migrations
{
    public partial class media_score_and_source : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categories",
                newName: "ID");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Media",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Media",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_Hash",
                table: "Media",
                column: "Hash",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Media_Hash",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Media");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Categories",
                newName: "Id");
        }
    }
}
