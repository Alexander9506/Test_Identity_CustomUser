using Microsoft.EntityFrameworkCore.Migrations;

namespace Test_CustomUserManagement.Migrations.EF
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileContainers",
                columns: table => new
                {
                    FileContainerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuiId = table.Column<string>(nullable: true),
                    FileDisplayName = table.Column<string>(nullable: true),
                    FileType = table.Column<string>(nullable: true),
                    FilePathFull = table.Column<string>(nullable: false),
                    FileDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileContainers", x => x.FileContainerId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileContainers");
        }
    }
}
