namespace MilestoneTracker.Application.Common.Interfaces;

using Domain.Entities;

public interface IParentRepository
{
    public Task<Parent?> GetWithChildrenAsync(long chatId, CancellationToken ct);
    public Task<bool> CreateAsync(Parent parent, CancellationToken ct);
    public Task<int> AddChildAsync(int parentId, Child child, CancellationToken ct);
    public Task<List<Child>> GetChildrenAsync(long chatId, CancellationToken ct);
    public Task<Child?> GetChildByIdAsync(int childId, CancellationToken ct);
    Task<Child?> GetByIdWithDeletedAsync(int childId, CancellationToken ct = default);
    Task<int> SoftDeleteAsync(long userChatId, int childId, CancellationToken ct = default);
    Task<int> RecoverAsync(int childId, CancellationToken cancellationToken = default);
}