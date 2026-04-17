namespace MilestoneTracker.Application.Common.Interfaces;

using Domain.Entities;

public interface IParentRepository
{
    public Task<Parent?> GetAsync(long chatId, CancellationToken ct);
    public Task<bool> CreateAsync(Parent parent, CancellationToken ct);
    public Task<int> AddChildAsync(int parentId, Child child, CancellationToken ct);
}