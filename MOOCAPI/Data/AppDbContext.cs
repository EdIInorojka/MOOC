using Microsoft.EntityFrameworkCore;
using MOOCAPI.Models;
using System.Reflection.Emit;

namespace MOOCAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected AppDbContext() : base() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MOOCDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Login).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Login)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.Password)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .HasMaxLength(100);

                entity.Property(u => u.FirstName)
                    .HasMaxLength(50);

                entity.Property(u => u.LastName)
                    .HasMaxLength(50);

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(u => u.RegistrationDate)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Конфигурация Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(c => c.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(c => c.Link)
                    .HasMaxLength(500);

                entity.Property(c => c.Description)
                    .HasMaxLength(2000);

                entity.Property(c => c.Language)
                    .HasMaxLength(50);

                entity.Property(c => c.StartDate)
                    .HasColumnType("date");

                entity.Property(c => c.EndDate)
                    .HasColumnType("date");

                entity.Property(c => c.Price)
                    .HasColumnType("decimal(18,2)");

                entity.Property(c => c.Reviews)
                    .HasColumnType("decimal(3,1)");
            });

            // Конфигурация Discipline
            modelBuilder.Entity<Discipline>(entity =>
            {
                entity.Property(d => d.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(d => d.StartDate)
                    .HasColumnType("date");

                entity.Property(d => d.EndDate)
                    .HasColumnType("date");
            });

            // Связь User-Course (многие-ко-многим)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Courses)
                .WithMany(c => c.Users)
                .UsingEntity(j => j.ToTable("UserCourses"));

            // Связь Course-Discipline (многие-ко-многим)
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Disciplines)
                .WithMany(d => d.Courses)
                .UsingEntity(j => j.ToTable("CourseDisciplines"));
            // Связь Course-University (многие-к-одному)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.University)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.UniversityId)
                .OnDelete(DeleteBehavior.SetNull);  // Если университет удалён, курс остаётся

            // Связь Course-Lecturer (многие-ко-многим)
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lecturers)
                .WithMany(l => l.Courses)
                .UsingEntity(j => j.ToTable("CourseLecturers"));
        }
    }
}