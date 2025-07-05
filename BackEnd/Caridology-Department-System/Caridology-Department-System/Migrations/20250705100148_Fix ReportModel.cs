using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caridology_Department_System.Migrations
{
    /// <inheritdoc />
    public partial class FixReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Doctors_DoctorID",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Patients_PatientID",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_DoctorID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DoctorID",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "PatientID",
                table: "Reports",
                newName: "AppointmentID");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_PatientID",
                table: "Reports",
                newName: "IX_Reports_AppointmentID");

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

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DoctorModelID",
                table: "Reports",
                column: "DoctorModelID");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PatientModelID",
                table: "Reports",
                column: "PatientModelID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Appointments_AppointmentID",
                table: "Reports",
                column: "AppointmentID",
                principalTable: "Appointments",
                principalColumn: "APPID",
                onDelete: ReferentialAction.Cascade);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Appointments_AppointmentID",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Doctors_DoctorModelID",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Patients_PatientModelID",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_DoctorModelID",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_PatientModelID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DoctorModelID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PatientModelID",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "AppointmentID",
                table: "Reports",
                newName: "PatientID");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_AppointmentID",
                table: "Reports",
                newName: "IX_Reports_PatientID");

            migrationBuilder.AddColumn<int>(
                name: "DoctorID",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DoctorID",
                table: "Reports",
                column: "DoctorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Doctors_DoctorID",
                table: "Reports",
                column: "DoctorID",
                principalTable: "Doctors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Patients_PatientID",
                table: "Reports",
                column: "PatientID",
                principalTable: "Patients",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
