using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caridology_Department_System.Migrations
{
    /// <inheritdoc />
    public partial class addressaddedtodoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Doctors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Doctors");
        }
    }
}
