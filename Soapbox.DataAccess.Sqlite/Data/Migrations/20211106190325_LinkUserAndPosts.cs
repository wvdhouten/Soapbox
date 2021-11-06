namespace Soapbox.Web.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class LinkUserAndPosts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PostPostCategory",
                table: "PostPostCategory");

            migrationBuilder.RenameColumn(
                name: "Author",
                table: "Posts",
                newName: "AuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostPostCategory",
                table: "PostPostCategory",
                columns: new[] { "CategoriesId", "PostsId" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_AuthorId",
                table: "Posts",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_AuthorId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostPostCategory",
                table: "PostPostCategory");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "Posts",
                newName: "Author");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostPostCategory",
                table: "PostPostCategory",
                columns: new[] { "PostsId", "CategoriesId" });
        }
    }
}
