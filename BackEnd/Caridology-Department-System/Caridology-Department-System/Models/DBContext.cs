using Caridology_Department_System.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Numerics;
using System.Xml;

namespace Caridology_Department_System.Models
{
    public class DBContext : DbContext  // Better naming convention
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        { }

        public DbSet<AdminModel> Admins { get; set; }
        public DbSet<PatientModel> Patients { get; set; }
        public DbSet<DoctorModel> Doctors { get; set; }
        public DbSet<MessageModel> Messages { get; set; }
        public DbSet<DoctorPhoneNumberModel> DoctorPhoneNumbers { get; set; }
        public DbSet<ReportModel> Reports { get; set; }
        public DbSet<AdminPhoneNumberModel> AdminPhoneNumbers { get; set; }
        public DbSet<AppointmentModel> Appointments { get; set; }
        public DbSet<PatientPhoneNumberModel> PatientPhoneNumbers { get; set; }
        public DbSet<ReportScanModel> ReportScans { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<StatusModel> Statuses { get; set; }
        /*  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
          {
              if (!optionsBuilder.IsConfigured)
              {
                  optionsBuilder.UseNpgsql("Host=ep-lively-cake-a2vlwa9y-pooler.eu-central-1.aws.neon.tech;Port=5432;" +
                      "Database=Cardiology_Department_DataBase;Username=postgres;Password=npg_RGBsKnp0ab3U");
              }
          }*/
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PatientModel>().ToTable("Patients");
            modelBuilder.Entity<DoctorModel>().ToTable("Doctors");
            modelBuilder.Entity<AdminModel>().ToTable("Admins");

            // Configure phone number relationships
            modelBuilder.Entity<PatientPhoneNumberModel>()
                .HasOne(p => p.Patient)
                .WithMany(p => p.PhoneNumbers)
                .HasForeignKey(p => p.PatientID);

            modelBuilder.Entity<DoctorPhoneNumberModel>()
                .HasOne(d => d.Doctor)
                .WithMany(d => d.PhoneNumbers)
                .HasForeignKey(d => d.DoctorID);

            modelBuilder.Entity<AdminPhoneNumberModel>()
                .HasOne(a => a.Admin)
                .WithMany(a => a.PhoneNumbers)
                .HasForeignKey(a => a.AdminID);

            // Seed data
            modelBuilder.Entity<RoleModel>().HasData(
                new RoleModel { RoleID = 1, RoleName = "Admin" },
                new RoleModel { RoleID = 2, RoleName = "Doctor" },
                new RoleModel { RoleID = 3, RoleName = "Patient" }
            );

            modelBuilder.Entity<StatusModel>().HasData(
                new StatusModel { StatusID = 1, Name = "Active" },
                new StatusModel { StatusID = 2, Name = "Inactive" },
                new StatusModel { StatusID = 3, Name = "Deleted" },
                new StatusModel { StatusID = 4, Name = "Pending" }
            );

            // Indexes
            modelBuilder.Entity<PatientModel>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<DoctorModel>()
                .HasIndex(d => d.Email)
                .IsUnique();

            modelBuilder.Entity<AdminModel>()
                .HasIndex(a => a.Email)
                .IsUnique();
        }
    }
}
