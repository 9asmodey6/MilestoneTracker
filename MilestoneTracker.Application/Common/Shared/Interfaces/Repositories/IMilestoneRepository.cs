namespace MilestoneTracker.Application.Common.Shared.Interfaces.Repositories;

using Domain.Entities.Milestones;

public interface IMilestoneRepository
{
    Task<int> AddAsync(Milestone milestone, CancellationToken cancellationToken);
}