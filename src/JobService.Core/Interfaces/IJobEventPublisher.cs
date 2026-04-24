using JobService.Core.Enums;

namespace JobService.Core.Interfaces;

public interface IJobEventPublisher
{
    Task PublishStatusChangedAsync(Guid jobReqId, string userId, JobStatus previousStatus, JobStatus newStatus);
    Task PublishApplicationSubmittedAsync(Guid jobReqId, string userId);
}
