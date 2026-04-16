namespace MilestoneTracker.Infrastructure.Persistence.Repositories;

using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class UserStateRepository(
    IAppDbContext dbContext) : IUserStateRepository
{
    public async Task<UserState?> GetByChatIdAsync(long chatId, CancellationToken ct)
    {
        var state = await dbContext.UserStates.FirstOrDefaultAsync(s => s.ChatId == chatId, ct);
        return state;
    }

    public async Task<bool> UpdateAsync(UserState state, CancellationToken ct)
    {
        int affectedRows = await dbContext.UserStates
            .Where(s => s.ChatId == state.ChatId)
            .ExecuteUpdateAsync(setter => setter
                    .SetProperty(u => u.State, state.State)
                    .SetProperty(u => u.StateData, state.StateData)
                    .SetProperty(u => u.UpdatedAt, state.UpdatedAt), 
                ct);
        
        return affectedRows > 0;
    }
}