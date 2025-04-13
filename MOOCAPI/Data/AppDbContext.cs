using Microsoft.EntityFrameworkCore;
using MOOCAPI.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MOOCAPI.Data
{
    public class AppDbContext : DbContext
    {
        // Основной конструктор для DI
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Конструктор для инструментов EF Core
        protected AppDbContext() : base() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }

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

                entity.Property(u => u.Login).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.FirstName).HasMaxLength(50);
                entity.Property(u => u.LastName).HasMaxLength(50);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.RegistrationDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // Конфигурация Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Link).HasMaxLength(500);
                entity.Property(c => c.Description).HasMaxLength(2000);
                entity.Property(c => c.Language).HasMaxLength(50);
                entity.Property(c => c.StartDate).HasColumnType("date");
                entity.Property(c => c.EndDate).HasColumnType("date");
            });

            // Настройка связи многие-ко-многим
            modelBuilder.Entity<User>()
                .HasMany(u => u.Courses)
                .WithMany(c => c.Users)
                .UsingEntity(j => j.ToTable("UserCourses"));
        }
    }
}
