namespace MilestoneTracker.Infrastructure.Persistence.Repositories;

using Application.Common.Interfaces;
using Application.Common.Shared.Interfaces.Repositories;
using Domain.Entities.Milestones;

public class MilestoneRepository(
    IAppDbContext dbContext): IMilestoneRepository
{
    public async Task<int> AddAsync(Milestone milestone, CancellationToken cancellationToken)
    {
        dbContext.Milestones.Add(milestone);
        await dbContext.SaveChangesAsync(cancellationToken);
        return milestone.Id;
    }
}