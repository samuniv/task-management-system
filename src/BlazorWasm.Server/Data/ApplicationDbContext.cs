using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BlazorWasm.Shared.Models;

namespace BlazorWasm.Server.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TaskItem entity
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(2000);
            entity.Property(t => t.Status).HasConversion<int>();
            entity.Property(t => t.Priority).HasConversion<int>();
            entity.Property(t => t.CreatedAt);
            entity.Property(t => t.UpdatedAt);

            // Configure relationships
            entity.HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Add index for performance
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.AssigneeId);
            entity.HasIndex(t => t.CreatorId);
            entity.HasIndex(t => t.IsDeleted);
        });

        // Configure Comment entity
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Content).HasMaxLength(2000);
            entity.Property(c => c.CreatedAt);

            // Configure relationships
            entity.HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add indexes
            entity.HasIndex(c => c.TaskId);
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.IsDeleted);
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Token).HasMaxLength(256);
            entity.Property(r => r.CreatedAt);
            entity.Property(r => r.ExpiresAt);

            // Configure relationship
            entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes
            entity.HasIndex(r => r.Token).IsUnique();
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.ExpiresAt);
        });

        // Configure global query filters for soft deletes
        modelBuilder.Entity<TaskItem>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(r => !r.IsRevoked);
    }
}
