using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBiweeklyRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BiweeklyRecordId",
                table: "Record",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BiweeklyRecords",
                columns: table => new
                {
                    BiweeklyRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalHours = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiweeklyRecords", x => x.BiweeklyRecordId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Record_BiweeklyRecordId",
                table: "Record",
                column: "BiweeklyRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Record_BiweeklyRecords_BiweeklyRecordId",
                table: "Record",
                column: "BiweeklyRecordId",
                principalTable: "BiweeklyRecords",
                principalColumn: "BiweeklyRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Record_BiweeklyRecords_BiweeklyRecordId",
                table: "Record");

            migrationBuilder.DropTable(
                name: "BiweeklyRecords");

            migrationBuilder.DropIndex(
                name: "IX_Record_BiweeklyRecordId",
                table: "Record");

            migrationBuilder.DropColumn(
                name: "BiweeklyRecordId",
                table: "Record");
        }
    }
}
