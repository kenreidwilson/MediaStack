using Microsoft.EntityFrameworkCore.Migrations;

namespace MediaStackCore.Migrations
{
    public partial class media_album_order : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlbumOrder",
                table: "Media",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlbumOrder",
                table: "Media");
        }
    }
}
