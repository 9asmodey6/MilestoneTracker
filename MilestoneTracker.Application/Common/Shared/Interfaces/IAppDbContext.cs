namespace MilestoneTracker.Application.Common.Interfaces;

using Domain.Entities;
using Domain.Entities.Milestones;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<Milestone> Milestones { get; }
    DbSet<MilestoneMedia> MilestoneMedia { get; }
    DbSet<Parent> Parents { get; }
    DbSet<Child> Children { get; }
    DbSet<UserState> UserStates { get; } 
    DbSet<ChildAccessToken> AccessTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}