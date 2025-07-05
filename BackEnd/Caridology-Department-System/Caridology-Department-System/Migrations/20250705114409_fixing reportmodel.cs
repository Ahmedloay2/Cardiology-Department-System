using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caridology_Department_System.Migrations
{
    /// <inheritdoc />
    public partial class fixingreportmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
            name: "FK_Reports_Doctors_DoctorModelID",
            table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Patients_PatientModelID",
                table: "Reports");

            // Drop indexes on those foreign keys
            migrationBuilder.DropIndex(
                name: "IX_Reports_DoctorModelID",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_PatientModelID",
                table: "Reports");

            // Drop the columns themselves
            migrationBuilder.DropColumn(
                name: "DoctorModelID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PatientModelID",
                table: "Reports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert: Add columns back
            migrationBuilder.AddColumn<int>(
                name: "DoctorModelID",
                table: "Reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PatientModelID",
                table: "Reports",
                type: "integer",
                nullable: true);

            // Re-add indexes
            migrationBuilder.CreateIndex(
                name: "IX_Reports_DoctorModelID",
                table: "Reports",
                column: "DoctorModelID");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PatientModelID",
                table: "Reports",
                column: "PatientModelID");

            // Re-add foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Doctors_DoctorModelID",
                table: "Reports",
                column: "DoctorModelID",
                principalTable: "Doctors",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Patients_PatientModelID",
                table: "Reports",
                column: "PatientModelID",
                principalTable: "Patients",
                principalColumn: "ID");
        }
    }
}
