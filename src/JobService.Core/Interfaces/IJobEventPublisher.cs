using JobService.Core.Enums;

namespace JobService.Core.Interfaces;

public interface IJobEventPublisher
{
    Task PublishStatusChangedAsync(Guid jobReqId, string userId, string? userEmail,
        string companyName, string roleTitle, JobStatus previousStatus, JobStatus newStatus);
    Task PublishApplicationSubmittedAsync(Guid jobReqId, string userId, string? userEmail,
        string companyName, string roleTitle);
}
