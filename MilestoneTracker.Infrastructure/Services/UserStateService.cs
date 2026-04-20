namespace MilestoneTracker.Infrastructure.Services;

using System.Text.Json;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

public class UserStateService(
    IUserStateRepository repository,
    ILogger<UserStateService> logger) : IUserStateService
{
    public async Task<UserState> GetAsync(long chatId, CancellationToken ct = default)
    {
        logger.LogInformation("Getting current state for chat {ChatId}", chatId);

        var state = await repository.GetByChatIdAsync(chatId, ct);

        if (state == null)
        {
            logger.LogInformation("No user state found for chat {ChatId}", chatId);

            return state ?? new UserState
            {
                ChatId = chatId,
                State = UserStateType.Idle
            };
        }

        logger.LogInformation("Returning current state for chat {ChatId}", chatId);

        return state;
    }

    public async Task AddAsync<T>(long chatId, UserStateType stateType, T? data = null, CancellationToken ct = default)
        where T : class
    {
        try
        {
            var newState = new UserState
            {
                ChatId = chatId,
                State = stateType,
                StateData = JsonSerializer.Serialize(data),
                UpdatedAt = DateTime.UtcNow
            };
            
            await repository.AddAsync(newState, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist state for {ChatId}", chatId);
            throw new InvalidOperationException($"Database error occured: {ex.Message}");
        }
    }

    public async Task UpdateAsync<T>(long chatId, UserStateType stateType, T? data, CancellationToken ct = default)
        where T : class
    {
        logger.LogInformation("Updating user state for chat {ChatId}", chatId);

        var existingState = await repository.GetByChatIdAsync(chatId, ct);

        string? jsonData = data != null
            ? JsonSerializer.Serialize(data)
            : null;

        if (existingState == null)
        {
            logger.LogInformation("No existing state for {ChatId}. Creating new record.", chatId);

            var newState = new UserState
            {
                ChatId = chatId,
                State = stateType,
                StateData = jsonData,
                UpdatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(newState, ct);
        }
        else
        {
            logger.LogInformation("Existing state found for {ChatId}. Modifying properties.", chatId);

            existingState.State = stateType;
            existingState.StateData = jsonData;
            existingState.UpdatedAt = DateTime.UtcNow;

            var isUpdated = await repository.UpdateAsync(existingState, ct);

            if (!isUpdated)
            {
                logger.LogWarning("Failed to update user state for chat {ChatId}", chatId);
                throw new InvalidOperationException($"Failed to update user state for chat {chatId}");
            }
        }
    }

    public async Task ResetAsync(long chatId, CancellationToken ct = default)
    {
        logger.LogInformation("Resetting state for for chat {ChatId}", chatId);

        await UpdateAsync<object>(chatId, UserStateType.Idle, null, ct);
    }
}