using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly string _currentUserId;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, string currentUserId = null)
            : base(options)
        {
            _currentUserId = currentUserId;
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<ChallengeCompletion> ChallengeCompletions { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Update> Updates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Team configurations
            builder.Entity<Team>(entity =>
            {
                entity.HasIndex(e => e.TeamNo).IsUnique();
                entity.Property(e => e.Color).HasMaxLength(7);
            });

            // Challenge configurations
            builder.Entity<Challenge>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ChallengeCompletion configurations
            builder.Entity<ChallengeCompletion>(entity =>
            {
                entity.HasOne(e => e.Team)
                    .WithMany(e => e.CompletedChallenges)
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Challenge)
                    .WithMany(e => e.Completions)
                    .HasForeignKey(e => e.ChallengeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TeamId, e.ChallengeId }).IsUnique();
            });

            // Update configurations
            builder.Entity<Update>(entity =>
            {
                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Challenge)
                    .WithMany()
                    .HasForeignKey(e => e.ChallengeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Announcement)
                    .WithMany()
                    .HasForeignKey(e => e.AnnouncementId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ChallengeCompletion)
                    .WithMany()
                    .HasForeignKey(e => e.ChallengeCompletionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ProcessAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ProcessAuditFields();
            return base.SaveChanges();
        }

        private void ProcessAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var currentTime = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = currentTime;
                        entry.Entity.CreatedBy = _currentUserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = currentTime;
                        entry.Entity.UpdatedBy = _currentUserId;
                        break;
                }
            }
        }
    }
}
