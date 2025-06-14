using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
namespace TodoAppNet
{
    public class TodoDbContext : DbContext
    {
        private readonly string _dbPath;

        public TodoDbContext(string dbPath)
        {
            _dbPath = dbPath;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ключи и связи

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Category>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tag>()
                .HasKey(t => t.Id);
            //modelBuilder.Entity<Tag>()
            //    .HasOne(t => t.User)
            //    .WithMany(u => u.Tags)
            //    .HasForeignKey(t => t.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskItem>()
                .HasKey(t => t.Id);
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new { tt.TaskId, tt.TagId });
            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Task)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            //modelBuilder.Entity<TaskTag>()
            //    .HasOne(tt => tt.Tag)
            //    .WithMany(t => t.TaskTags)
            //    .HasForeignKey(tt => tt.TagId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reminder>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.Task)
                .WithMany(t => t.Reminders)
                .HasForeignKey(r => r.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы
            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.UserId);
            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.CategoryId);
            modelBuilder.Entity<TaskTag>()
                .HasIndex(tt => tt.TagId);
            modelBuilder.Entity<Reminder>()
                .HasIndex(r => r.TaskId);

            // Триггер обновления UpdatedAt можно реализовать в коде при сохранении
        }
    }
}
