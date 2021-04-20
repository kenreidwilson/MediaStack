using Microsoft.EntityFrameworkCore.Migrations;

namespace MediaStackCore.Migrations
{
    public partial class album_unique_id_artist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Albums_Name",
                table: "Albums");

            migrationBuilder.AddColumn<int>(
                name: "ArtistID",
                table: "Albums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistID",
                table: "Albums",
                column: "ArtistID");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Name_ArtistID",
                table: "Albums",
                columns: new[] { "Name", "ArtistID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Artists_ArtistID",
                table: "Albums",
                column: "ArtistID",
                principalTable: "Artists",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Artists_ArtistID",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_ArtistID",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_Name_ArtistID",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ArtistID",
                table: "Albums");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Name",
                table: "Albums",
                column: "Name",
                unique: true);
        }
    }
}
