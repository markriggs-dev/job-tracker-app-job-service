using JobService.Core.DTOs;
using JobService.Core.Enums;
using JobService.Core.Interfaces;
using JobService.Core.Models;

namespace JobService.Core.Services;

public class JobRequisitionService
{
    private readonly IJobRequisitionRepository _repository;
    private readonly IJobEventPublisher _eventPublisher;

    public JobRequisitionService(
        IJobRequisitionRepository repository,
        IJobEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IEnumerable<JobRequisitionListResponse>> GetAllAsync(string userId)
    {
        var reqs = await _repository.GetAllByUserAsync(userId);
        return reqs.Select(MapToListResponse);
    }

    public async Task<JobRequisitionResponse?> GetByIdAsync(Guid id, string userId)
    {
        var req = await _repository.GetByIdAsync(id, userId);
        return req is null ? null : MapToResponse(req);
    }

    public async Task<IEnumerable<JobRequisitionListResponse>> SearchAsync(
        string userId, string? keyword, JobStatus? status)
    {
        var reqs = await _repository.SearchAsync(userId, keyword, status);
        return reqs.Select(MapToListResponse);
    }

    // Publishes to Kafka — consumer writes to DB. Returns 202 immediately.
    public async Task<JobRequisitionAcceptedResponse> CreateAsync(
        string userId, string? userEmail, CreateJobRequisitionRequest request)
    {
        var jobReqId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        await _eventPublisher.PublishJobCreatedAsync(
            jobReqId, userId, userEmail,
            request.CompanyName, request.RoleTitle,
            request.SourceUrl, request.CompanyCareerPortalUrl, request.JobDescription,
            request.DateDiscovered, request.ApplicationExpiryDate,
            occurredAt);

        return new JobRequisitionAcceptedResponse(jobReqId, "Job application queued for processing");
    }

    // Validates ownership synchronously, then publishes to Kafka — consumer writes to DB.
    public async Task<JobRequisitionAcceptedResponse?> UpdateAsync(
        Guid id, string userId, string? userEmail, UpdateJobRequisitionRequest request)
    {
        var existing = await _repository.GetByIdAsync(id, userId);
        if (existing is null) return null;

        var occurredAt = DateTimeOffset.UtcNow;

        await _eventPublisher.PublishJobUpdatedAsync(
            id, userId, userEmail,
            request.CompanyName, request.RoleTitle,
            request.SourceUrl, request.CompanyCareerPortalUrl, request.JobDescription,
            request.DateDiscovered, request.ApplicationExpiryDate, request.DateSubmitted,
            occurredAt);

        return new JobRequisitionAcceptedResponse(id, "Job update queued for processing");
    }

    public async Task<JobRequisitionResponse?> UpdateStatusAsync(
        Guid id, string userId, JobStatus newStatus)
    {
        var existing = await _repository.GetByIdAsync(id, userId);
        if (existing is null) return null;

        existing.Status = newStatus;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        if (newStatus == JobStatus.Applied)
            existing.DateSubmitted = DateOnly.FromDateTime(DateTime.UtcNow);

        var updated = await _repository.UpdateAsync(existing);
        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        return await _repository.DeleteAsync(id, userId);
    }

    private static string GetStatusDisplay(JobStatus status) => status switch
    {
        JobStatus.Discovered => "Discovered",
        JobStatus.Applied => "Applied",
        JobStatus.InProgress => "In Progress",
        JobStatus.WaitingOnResponse => "Waiting on Response",
        JobStatus.InterviewScheduled => "Interview Scheduled",
        JobStatus.OfferReceived => "Offer Received",
        JobStatus.Closed => "Closed",
        JobStatus.Withdrawn => "Withdrawn",
        _ => status.ToString()
    };

    private static JobRequisitionResponse MapToResponse(JobRequisition req) => new(
        req.Id,
        req.CompanyName,
        req.RoleTitle,
        req.SourceUrl,
        req.CompanyCareerPortalUrl,
        req.JobDescription,
        req.Status,
        GetStatusDisplay(req.Status),
        req.DateDiscovered,
        req.ApplicationExpiryDate,
        req.DateSubmitted,
        req.CreatedAt,
        req.UpdatedAt
    );

    private static JobRequisitionListResponse MapToListResponse(JobRequisition req) => new(
        req.Id,
        req.CompanyName,
        req.RoleTitle,
        req.Status,
        GetStatusDisplay(req.Status),
        req.DateDiscovered,
        req.DateSubmitted,
        req.ApplicationExpiryDate
    );
}
