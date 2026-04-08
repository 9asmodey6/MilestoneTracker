namespace MilestoneTracker.Infrastructure.Persistence;

using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Entities.Milestones;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Milestone> Milestones { get; set; }

    public DbSet<MilestoneMedia> MilestoneMedia { get; set; }

    public DbSet<Parent> Parents { get; set; }

    public DbSet<Child> Children { get; set; }

    public DbSet<UserState> UserStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}