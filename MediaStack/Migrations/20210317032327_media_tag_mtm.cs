using Microsoft.EntityFrameworkCore.Migrations;

namespace MediaStack_Library.Migrations
{
    public partial class media_tag_mtm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Media_MediaID",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_MediaID",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "MediaID",
                table: "Tags");

            migrationBuilder.CreateTable(
                name: "MediaTag",
                columns: table => new
                {
                    MediaID = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaTag", x => new { x.MediaID, x.TagsID });
                    table.ForeignKey(
                        name: "FK_MediaTag_Media_MediaID",
                        column: x => x.MediaID,
                        principalTable: "Media",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaTag_Tags_TagsID",
                        column: x => x.TagsID,
                        principalTable: "Tags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaTag_TagsID",
                table: "MediaTag",
                column: "TagsID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaTag");

            migrationBuilder.AddColumn<int>(
                name: "MediaID",
                table: "Tags",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_MediaID",
                table: "Tags",
                column: "MediaID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Media_MediaID",
                table: "Tags",
                column: "MediaID",
                principalTable: "Media",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
