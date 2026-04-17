namespace MilestoneTracker.Application.Common.Interfaces;

using Domain.Entities;
using Domain.Enums;

public interface IUserStateService
{
    Task<UserState> GetAsync(long chatId, CancellationToken ct = default);
    
    Task<bool> AddAsync(UserState state, CancellationToken ct);
    
    Task UpdateAsync<T>(long chatId, UserStateType stateType, T? data, CancellationToken ct = default) where T : class;

    Task ResetAsync(long chatId, CancellationToken ct = default);
}