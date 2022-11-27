namespace Soapbox.Web.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddPostCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Slug = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostPostCategory",
                columns: table => new
                {
                    PostsId = table.Column<string>(type: "TEXT", nullable: false),
                    CategoriesId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPostCategory", x => new { x.PostsId, x.CategoriesId });
                    table.ForeignKey(
                        name: "FK_PostPostCategory_Posts_PostsId",
                        column: x => x.PostsId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostPostCategory_PostCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "PostCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostPostCategory_PostsId",
                table: "PostPostCategory",
                column: "PostsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostPostCategory");

            migrationBuilder.DropTable(
                name: "PostCategories");
        }
    }
}
