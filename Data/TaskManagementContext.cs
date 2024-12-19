using System.Data.Entity;
using TaskManagement.Models;

namespace TaskManagement.Data
{
    public class TaskManagementContext : DbContext
    {
        public TaskManagementContext() : base("name=TaskManagementConnectionString") { }

        public DbSet<AccountData> AccountData { get; set; }
        public DbSet<Backup> Backups { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Metadataa> Metadata { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<TaskDistribution> TaskDistributions { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
