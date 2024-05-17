using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWbs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "WBS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "WBS",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
