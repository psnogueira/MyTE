using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTE.Data.Migrations
{
    /// <inheritdoc />
    public partial class Departmente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmenteDepartmentId",
                table: "Employee",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Departmente",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departmente", x => x.DepartmentId);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Departmente_DepartmenteDepartmentId",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "Departmente");

            migrationBuilder.DropIndex(
                name: "IX_Employee_DepartmenteDepartmentId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "DepartmenteDepartmentId",
                table: "Employee");
        }
    }
}
