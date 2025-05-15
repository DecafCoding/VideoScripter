using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using VideoScripter.Data.Entities;

namespace VideoScripter.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Project> Projects { get; set; } = default!;
    public DbSet<Video> Videos { get; set; } = default!;
    public DbSet<Script> Scripts { get; set; } = default!;
    public DbSet<Channel> Channels { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<TranscriptTopic> TranscriptTopics { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure relationships and constraints here
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Videos)
            .WithOne(v => v.Project)
            .HasForeignKey(v => v.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Scripts)
            .WithOne(s => s.Project)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
