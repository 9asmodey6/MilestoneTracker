namespace MilestoneTracker.Infrastructure.Persistence.Repositories;

using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class ParentRepository(
    IAppDbContext dbContext) : IParentRepository
{
    public async Task<Parent?> GetWithChildrenAsync(long chatId, CancellationToken ct)
    {
        return await dbContext.Parents
            .AsNoTracking()
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.ChatId == chatId, ct);
    }

    public async Task<bool> CreateAsync(Parent parent, CancellationToken ct)
    {
        dbContext.Parents.Add(parent);

        return await dbContext.SaveChangesAsync(ct) > 0;
    }

    public async Task<int> AddChildAsync(int parentId, Child child, CancellationToken ct)
    {
        var parent = await dbContext.Parents
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.Id == parentId, ct);

        if (parent == null)
        {
            throw new InvalidOperationException($"Parent with Id {parentId} not found");
        }

        parent.Children.Add(child);
        await dbContext.SaveChangesAsync(ct);
        return child.Id;
    }

    public async Task<List<Child>> GetChildrenAsync(long chatId, CancellationToken ct)
    {
        return await dbContext.Children
            .Where(c => c.Parents.Any(p => p.ChatId == chatId))
            .ToListAsync(ct);
    }

    public Task<Child?> GetChildByIdAsync(int childId, CancellationToken ct)
    {
        return dbContext.Children.FirstOrDefaultAsync(c => c.Id == childId, ct);
    }

    public async Task<Child?> GetByIdWithDeletedAsync(int childId, CancellationToken ct = default)
    {
        return await dbContext.Children
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == childId, ct);
    }

    public async Task<int> SoftDeleteAsync(long userChatId, int childId, CancellationToken ct = default)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            var now = DateTime.UtcNow;

            await dbContext.Milestones
                .Where(m => m.ChildId == childId)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.IsDeleted, true)
                        .SetProperty(m => m.DeletedAt, now)
                        .SetProperty(m => m.DeletedBy, userChatId),
                    ct);
            
            var affectedRows = await dbContext.Children
                .Where(c => c.Id == childId)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.IsDeleted, true)
                        .SetProperty(c => c.DeletedAt, now)
                        .SetProperty(c => c.DeletedBy, userChatId),
                    ct);
            
            await transaction.CommitAsync(ct);
        
            return affectedRows;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<int> RecoverAsync(int childId, CancellationToken cancellationToken = default)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;

            await dbContext.Milestones
                .IgnoreQueryFilters()
                .Where(m => m.ChildId == childId)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.IsDeleted, false)
                        .SetProperty(m => m.DeletedAt, (DateTime?)null)
                        .SetProperty(m => m.DeletedBy, (long?)null),
                    cancellationToken);
            
            var affectedRows = await dbContext.Children
                .IgnoreQueryFilters()
                .Where(c => c.Id == childId)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.IsDeleted, false)
                        .SetProperty(c => c.DeletedAt, (DateTime?)null)
                        .SetProperty(c => c.DeletedBy, (long?)null),
                    cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        
            return affectedRows;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}