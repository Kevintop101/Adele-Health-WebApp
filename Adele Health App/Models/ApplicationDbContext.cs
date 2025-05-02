using Microsoft.EntityFrameworkCore;
using Adele_Health_App.Models;

namespace Adele_Health_App.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSet for log entries
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<LifestyleEntry> LifestyleEntries { get; set; }
    }
}