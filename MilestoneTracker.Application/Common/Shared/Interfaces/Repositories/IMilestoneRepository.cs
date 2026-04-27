namespace MilestoneTracker.Application.Common.Shared.Interfaces.Repositories;

using Domain.Entities.Milestones;
using Domain.Enums;

public interface IMilestoneRepository
{
    Task<int> AddAsync(Milestone milestone, CancellationToken cancellationToken);
    Task<(List<Milestone> Items, int TotalCount)> GetPaginatedAsync(
        int childId,
        int pageNumber,
        int pageSize,
        MilestoneCategory? category,
        DateTime? specificDate,
        CancellationToken ct);
}