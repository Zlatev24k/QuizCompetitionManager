using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Competition> Competitions => Set<Competition>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<CompetitionRegistration> CompetitionRegistrations => Set<CompetitionRegistration>();
        public DbSet<RoundScore> RoundScores => Set<RoundScore>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Един потребител -> един отбор 
            builder.Entity<Team>()
                .HasIndex(t => t.OwnerUserId)
                .IsUnique();

            builder.Entity<Team>()
                .HasIndex(t => t.Name)
                .IsUnique();

            // TeamMember
            builder.Entity<TeamMember>()
                .HasOne(m => m.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Registration
            builder.Entity<CompetitionRegistration>()
                .HasIndex(r => new { r.CompetitionId, r.TeamId })
                .IsUnique();

            builder.Entity<CompetitionRegistration>()
                .HasOne(r => r.Competition)
                .WithMany(c => c.Registrations)
                .HasForeignKey(r => r.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CompetitionRegistration>()
                .HasOne(r => r.Team)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoundScore
            builder.Entity<RoundScore>()
                .HasIndex(s => new { s.CompetitionRegistrationId, s.RoundNumber })
                .IsUnique();

            builder.Entity<RoundScore>()
                .HasOne(s => s.CompetitionRegistration)
                .WithMany(r => r.RoundScores)
                .HasForeignKey(s => s.CompetitionRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
