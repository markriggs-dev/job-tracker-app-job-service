using JobService.Core.Enums;
using JobService.Core.Interfaces;
using JobService.Core.Models;
using JobService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobService.Infrastructure.Repositories;

public class JobRequisitionRepository : IJobRequisitionRepository
{
    private readonly JobServiceDbContext _context;

    public JobRequisitionRepository(JobServiceDbContext context)
    {
        _context = context;
    }

    public async Task<JobRequisition?> GetByIdAsync(Guid id, string userId)
    {
        return await _context.JobRequisitions
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);
    }

    public async Task<IEnumerable<JobRequisition>> GetAllByUserAsync(string userId)
    {
        return await _context.JobRequisitions
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.DateDiscovered)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobRequisition>> SearchAsync(
        string userId, string? keyword, JobStatus? status)
    {
        var query = _context.JobRequisitions
            .Where(j => j.UserId == userId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lower = keyword.ToLower();
            query = query.Where(j =>
                j.CompanyName.ToLower().Contains(lower) ||
                j.RoleTitle.ToLower().Contains(lower));
        }

        if (status.HasValue)
            query = query.Where(j => j.Status == status.Value);

        return await query
            .OrderByDescending(j => j.DateDiscovered)
            .ToListAsync();
    }

    public async Task<JobRequisition> CreateAsync(JobRequisition jobRequisition)
    {
        _context.JobRequisitions.Add(jobRequisition);
        await _context.SaveChangesAsync();
        return jobRequisition;
    }

    public async Task<JobRequisition> UpdateAsync(JobRequisition jobRequisition)
    {
        jobRequisition.UpdatedAt = DateTimeOffset.UtcNow;
        _context.JobRequisitions.Update(jobRequisition);
        await _context.SaveChangesAsync();
        return jobRequisition;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var existing = await _context.JobRequisitions
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (existing is null) return false;

        // Soft delete - never permanently remove records
        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
