namespace MilestoneTracker.Application.Common.Shared.Interfaces.Services;

using Domain.Entities;
using Models;

public interface IChildAccessTokenService
{
    Task<List<ChildAccessToken>> GetChildAccessTokensAsync(int childId, CancellationToken ct = default);

    Task<Result<ChildAccessToken>> GenerateTokenAsync(int childId, int creatorId, int validityHours = 24,
        CancellationToken ct = default);

    Task<Result> ConsumeTokenAsync(string token, long parentChatId, CancellationToken ct);
    Task<int> ClearInvalidTokensAsync(CancellationToken ct = default);
}