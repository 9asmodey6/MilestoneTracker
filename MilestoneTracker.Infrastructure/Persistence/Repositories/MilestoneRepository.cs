namespace MilestoneTracker.Infrastructure.Persistence.Repositories;

using Application.Common.Interfaces;
using Application.Common.Shared.Interfaces.Repositories;
using Domain.Entities.Milestones;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class MilestoneRepository(
    IAppDbContext dbContext): IMilestoneRepository
{
    public async Task<int> AddAsync(Milestone milestone, CancellationToken cancellationToken)
    {
        dbContext.Milestones.Add(milestone);
        await dbContext.SaveChangesAsync(cancellationToken);
        return milestone.Id;
    }
    
    public async Task<(List<Milestone> Items, int TotalCount)> GetPaginatedAsync(
        int childId, 
        int pageNumber, 
        int pageSize = 5,
        MilestoneCategory? category = null,
        DateTime? specificDate = null,
        CancellationToken ct = default)
    {
        var query = dbContext.Milestones
            .Where(m => m.ChildId == childId && !m.IsDeleted);
        
        if (category.HasValue)
        {
            query = query.Where(m => m.Category == category.Value);
        }
        
        if (specificDate.HasValue)
        {
            var date = specificDate.Value.Date;
            query = query.Where(m => m.OccurredAt.Date == date);
        }
        
        int totalCount = await query.CountAsync(ct);
        
        var items = await query
            .OrderByDescending(m => m.OccurredAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        
        return (items, totalCount);
    }

    public async Task<Milestone?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await dbContext.Milestones
            .Include(m => m.MediaFiles)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task UpdateAsync(Milestone milestone, CancellationToken ct = default)
    {
        dbContext.Milestones.Update(milestone);
        await dbContext.SaveChangesAsync(ct);
    }
}