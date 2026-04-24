using JobService.Core.Enums;
using JobService.Core.Models;

namespace JobService.Core.Interfaces;

public interface IJobRequisitionRepository
{
    Task<JobRequisition?> GetByIdAsync(Guid id, string userId);
    Task<IEnumerable<JobRequisition>> GetAllByUserAsync(string userId);
    Task<IEnumerable<JobRequisition>> SearchAsync(string userId, string? keyword, JobStatus? status);
    Task<JobRequisition> CreateAsync(JobRequisition jobRequisition);
    Task<JobRequisition> UpdateAsync(JobRequisition jobRequisition);
    Task<bool> DeleteAsync(Guid id, string userId);
}
