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
}