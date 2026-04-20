namespace MilestoneTracker.Application.Common.Interfaces;

using Domain.Entities;

public interface IUserStateRepository
{
    Task<UserState?> GetByChatIdAsync(long chatId, CancellationToken ct);
    Task<bool> UpdateAsync(UserState state, CancellationToken ct);
    Task AddAsync(UserState state, CancellationToken ct);
}