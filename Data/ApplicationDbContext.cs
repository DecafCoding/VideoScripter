using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using VideoScripter.Data.Entities;
using VideoScripter.Data.Common;

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

        // Project relationships
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasMany(p => p.Videos)
                .WithOne(v => v.Project)
                .HasForeignKey(v => v.ProjectId)
                .OnDelete(DeleteBehavior.SetNull); // Changed from Cascade to avoid orphaned videos

            entity.HasMany(p => p.Scripts)
                .WithOne(s => s.Project)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add user relationship
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Video relationships
        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasOne(v => v.Channel)
                .WithMany(c => c.Videos)
                .HasForeignKey(v => v.ChannelId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting channels with videos

            entity.HasMany(v => v.TranscriptTopics)
                .WithOne(tt => tt.Video)
                .HasForeignKey(tt => tt.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure nullable ProjectId
            entity.Property(v => v.ProjectId)
                .IsRequired(false);
        });

        // TranscriptTopic relationships
        modelBuilder.Entity<TranscriptTopic>(entity =>
        {
            entity.HasOne(tt => tt.Video)
                .WithMany(v => v.TranscriptTopics)
                .HasForeignKey(tt => tt.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Script relationships
        modelBuilder.Entity<Script>(entity =>
        {
            entity.HasOne(s => s.Project)
                .WithMany(p => p.Scripts)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Channel-Category many-to-many relationship
        modelBuilder.Entity<Channel>()
            .HasMany(c => c.Categories)
            .WithMany(cat => cat.Channels)
            .UsingEntity(j => j.ToTable("ChannelCategories"));

        // Configure string length constraints for better performance
        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(200);
            entity.Property(p => p.Topic).HasMaxLength(500);
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.Property(v => v.YTId).HasMaxLength(50);
            entity.Property(v => v.Title).HasMaxLength(500);
            entity.Property(v => v.Description).HasMaxLength(5000);
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.Property(c => c.YTId).HasMaxLength(50);
            entity.Property(c => c.Title).HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(2000);
            entity.Property(c => c.UploadsPlaylistId).HasMaxLength(50);
            entity.Property(c => c.ThumbnailURL).HasMaxLength(500);
            entity.Property(c => c.Notes).HasMaxLength(2000);
        });

        modelBuilder.Entity<Script>(entity =>
        {
            entity.Property(s => s.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<TranscriptTopic>(entity =>
        {
            entity.Property(tt => tt.TopicSummary).HasMaxLength(1000);
        });

        // Add indexes for better performance
        modelBuilder.Entity<Video>()
            .HasIndex(v => v.YTId)
            .IsUnique();

        modelBuilder.Entity<Channel>()
            .HasIndex(c => c.YTId)
            .IsUnique();

        modelBuilder.Entity<Project>()
            .HasIndex(p => p.UserId);

        // Configure BaseEntity properties for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType, builder =>
            {
                builder.Property(nameof(BaseEntity.CreatedBy)).HasMaxLength(256);
                builder.Property(nameof(BaseEntity.LastModifiedBy)).HasMaxLength(256);

                // Add index on IsDeleted for soft delete queries
                builder.HasIndex(nameof(BaseEntity.IsDeleted));
            });
        }
    }
}