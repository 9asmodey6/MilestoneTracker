namespace MilestoneTracker.Infrastructure.Services;

using Application.Common.Interfaces;
using Application.Common.Shared.Interfaces.Services;
using Application.Common.Shared.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

public class ChildAccessTokenService(IAppDbContext dbContext, ILogger<ChildAccessTokenService> logger) : IChildAccessTokenService
{
    public async Task<Result<ChildAccessToken>> GenerateTokenAsync(int childId, int creatorId, int validityHours = 24,  CancellationToken ct = default)
    {
        var child = await dbContext.Children
            .Include(c => c.Parents)
            .FirstOrDefaultAsync(c => c.Id == childId, ct);

        if (child == null)
            return Result<ChildAccessToken>.Failure("Ребёнок не найден");

        if (child.Parents.All(p => p.Id != creatorId))
            return Result<ChildAccessToken>.Failure("У вас нет прав для управления доступом к этому ребёнку");

        var tokenString = GenerateSecureToken();
        
        var token = new ChildAccessToken
        {
            Id = Guid.NewGuid(),
            ChildId = childId,
            CreatorId = creatorId,
            Token = tokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(validityHours),
            MaxUses = 1,
            CurrentUses = 0,
            IsUsed = false
        };

        dbContext.AccessTokens.Add(token);
        await dbContext.SaveChangesAsync(ct);

        return Result<ChildAccessToken>.Success(token);
    }

    public async Task<Result> ConsumeTokenAsync(string tokenString, long parentChatId, CancellationToken ct)
    {
        var token = await dbContext.AccessTokens
            .Include(t => t.Child)
            .ThenInclude(c => c.Parents)
            .FirstOrDefaultAsync(t => t.Token == tokenString, ct);

        if (token == null)
            return Result.Failure("Неверный токен. Проверьте правильность ввода.");

        if (!token.IsValid())
            return Result.Failure("Токен недействителен, просрочен или уже был использован.");

        var parent = await dbContext.Parents
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.ChatId == parentChatId, ct);

        if (parent == null)
            return Result.Failure("Вы не зарегистрированы в системе. Пожалуйста, начните с команды /start.");

        if (token.Child.Parents.Any(p => p.ChatId == parentChatId))
            return Result.Failure("У вас уже есть доступ к этому ребёнку.");
        
        token.Child.Parents.Add(parent);
        
        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;
        token.UsedByParentId = parent.Id;
        token.CurrentUses++;

        await dbContext.SaveChangesAsync(CancellationToken.None);

        return Result.Success();
    }

    public async Task<int> ClearInvalidTokensAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var affectedRows =  await dbContext.AccessTokens
            .Where(t => t.IsUsed 
                        || t.ExpiresAt < now 
                        || t.CurrentUses >= t.MaxUses)
            .ExecuteDeleteAsync(ct);
        
        logger.LogInformation("Clear invalid tokens: {Count}", affectedRows);
        return affectedRows;
    }

    private static string GenerateSecureToken()
    {
        var buffer = new byte[6];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer).ToLower();
    }
}
