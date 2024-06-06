using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentToBiweeklyRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "BiweeklyRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BiweeklyRecords_DepartmentId",
                table: "BiweeklyRecords",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_BiweeklyRecords_Department_DepartmentId",
                table: "BiweeklyRecords",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BiweeklyRecords_Department_DepartmentId",
                table: "BiweeklyRecords");

            migrationBuilder.DropIndex(
                name: "IX_BiweeklyRecords_DepartmentId",
                table: "BiweeklyRecords");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "BiweeklyRecords");
        }
    }
}
