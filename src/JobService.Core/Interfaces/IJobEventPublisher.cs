namespace JobService.Core.Interfaces;

public interface IJobEventPublisher
{
    Task PublishJobCreatedAsync(
        Guid jobReqId, string userId, string? userEmail,
        string companyName, string roleTitle,
        string? sourceUrl, string? companyCareerPortalUrl, string? jobDescription,
        DateOnly dateDiscovered, DateOnly? applicationExpiryDate, DateTime? interviewDate,
        DateTimeOffset occurredAt);

    Task PublishJobUpdatedAsync(
        Guid jobReqId, string userId, string? userEmail,
        string companyName, string roleTitle,
        string? sourceUrl, string? companyCareerPortalUrl, string? jobDescription,
        DateOnly dateDiscovered, DateOnly? applicationExpiryDate, DateOnly? dateSubmitted, DateTime? interviewDate,
        DateTimeOffset occurredAt);
}
