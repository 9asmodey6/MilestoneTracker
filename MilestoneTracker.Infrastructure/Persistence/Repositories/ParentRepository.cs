namespace MilestoneTracker.Infrastructure.Persistence.Repositories;

using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class ParentRepository(
    AppDbContext dbContext) : IParentRepository
{
    public async Task<Parent?> GetAsync(long chatId, CancellationToken ct)
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
}