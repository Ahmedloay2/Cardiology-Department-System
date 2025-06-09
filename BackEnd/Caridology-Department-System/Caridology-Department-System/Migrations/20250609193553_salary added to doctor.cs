using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caridology_Department_System.Migrations
{
    /// <inheritdoc />
    public partial class salaryaddedtodoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Salary",
                table: "Doctors",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Doctors");
        }
    }
}
