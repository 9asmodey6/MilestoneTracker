namespace MilestoneTracker.Infrastructure.Persistence;

using Domain.Entities;
using Domain.Entities.Milestones;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Milestone> Milestones { get; set; }
    
    public DbSet<MilestoneMedia>  MilestoneMedia { get; set; }
    
    public DbSet<Parent> Parents { get; set; }
    
    public DbSet<Child> Children { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}