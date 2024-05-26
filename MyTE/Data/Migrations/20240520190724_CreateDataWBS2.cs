using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTE.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateDataWBS2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "WBS",
                columns: new[] { "WBSId", "Code", "Desc", "Type" },
                values: new object[,] {
                    { 1, "A805732", "Férias", 2 },
                    { 2, "D789012", "Day-off", 2 },
                    { 3, "I345678", "Implementação", 1 },
                    { 4, "D901234", "Desenvolvimento", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
