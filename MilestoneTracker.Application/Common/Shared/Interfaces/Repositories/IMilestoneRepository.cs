namespace MilestoneTracker.Application.Common.Shared.Interfaces.Repositories;

using Domain.Entities.Milestones;
using Domain.Enums;

public interface IMilestoneRepository
{
    Task<int> AddAsync(Milestone milestone, CancellationToken cancellationToken);
    Task<(List<Milestone> Items, int TotalCount)> GetPaginatedAsync(
        int childId,
        int pageNumber,
        int pageSize = 5,
        MilestoneCategory? category = null,
        DateTime? specificDate = null,
        CancellationToken ct = default);

    Task<Milestone?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Milestone?> GetByIdWithDeletedAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(Milestone milestone, CancellationToken ct = default);
    Task<int> SoftDeleteAsync(long userChatId, int milestoneId, CancellationToken ct = default);
}