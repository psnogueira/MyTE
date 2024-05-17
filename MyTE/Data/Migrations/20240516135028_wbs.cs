using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTE.Data.Migrations
{
    /// <inheritdoc />
    public partial class wbs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Departmente_DepartmenteDepartmentId",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_DepartmenteDepartmentId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "DepartmenteDepartmentId",
                table: "Employee");

            migrationBuilder.CreateTable(
                name: "WBS",
                columns: table => new
                {
                    WBSId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WBS", x => x.WBSId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WBS");

            migrationBuilder.AddColumn<int>(
                name: "DepartmenteDepartmentId",
                table: "Employee",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DepartmenteDepartmentId",
                table: "Employee",
                column: "DepartmenteDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Departmente_DepartmenteDepartmentId",
                table: "Employee",
                column: "DepartmenteDepartmentId",
                principalTable: "Departmente",
                principalColumn: "DepartmentId");
        }
    }
}
