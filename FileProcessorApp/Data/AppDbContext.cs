using FileProcessorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FileProcessorApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<FileEntry> Files => Set<FileEntry>();
        public DbSet<FileStatistics> Statistics => Set<FileStatistics>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileEntry>().HasMany(f => f.Statistics).WithOne(s => s.FileEntry).HasForeignKey(s => s.FileEntryId);
        }

    }
}
