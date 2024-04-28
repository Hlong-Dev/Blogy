using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSo2.Migrations
{
    public partial class v5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blog_AspNetUsers_UserName",
                table: "Blog");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Blog",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Blog_UserName",
                table: "Blog",
                newName: "IX_Blog_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blog_AspNetUsers_UserId",
                table: "Blog",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blog_AspNetUsers_UserId",
                table: "Blog");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Blog",
                newName: "UserName");

            migrationBuilder.RenameIndex(
                name: "IX_Blog_UserId",
                table: "Blog",
                newName: "IX_Blog_UserName");

            migrationBuilder.AddForeignKey(
                name: "FK_Blog_AspNetUsers_UserName",
                table: "Blog",
                column: "UserName",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
