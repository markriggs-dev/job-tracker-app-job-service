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

    public async Task<JobRequisitionResponse> CreateAsync(
        string userId, CreateJobRequisitionRequest request)
    {
        var req = new JobRequisition
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = request.CompanyName,
            RoleTitle = request.RoleTitle,
            SourceUrl = request.SourceUrl,
            CompanyCareerPortalUrl = request.CompanyCareerPortalUrl,
            JobDescription = request.JobDescription,
            Status = JobStatus.Discovered,
            DateDiscovered = request.DateDiscovered,
            ApplicationExpiryDate = request.ApplicationExpiryDate,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.CreateAsync(req);
        return MapToResponse(created);
    }

    public async Task<JobRequisitionResponse?> UpdateAsync(
        Guid id, string userId, UpdateJobRequisitionRequest request)
    {
        var existing = await _repository.GetByIdAsync(id, userId);
        if (existing is null) return null;

        existing.CompanyName = request.CompanyName;
        existing.RoleTitle = request.RoleTitle;
        existing.SourceUrl = request.SourceUrl;
        existing.CompanyCareerPortalUrl = request.CompanyCareerPortalUrl;
        existing.JobDescription = request.JobDescription;
        existing.DateDiscovered = request.DateDiscovered;
        existing.ApplicationExpiryDate = request.ApplicationExpiryDate;
        existing.DateSubmitted = request.DateSubmitted;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return MapToResponse(updated);
    }

    public async Task<JobRequisitionResponse?> UpdateStatusAsync(
        Guid id, string userId, JobStatus newStatus)
    {
        var existing = await _repository.GetByIdAsync(id, userId);
        if (existing is null) return null;

        var previousStatus = existing.Status;
        existing.Status = newStatus;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        if (newStatus == JobStatus.Applied)
            existing.DateSubmitted = DateOnly.FromDateTime(DateTime.UtcNow);

        var updated = await _repository.UpdateAsync(existing);

        await _eventPublisher.PublishStatusChangedAsync(id, userId, previousStatus, newStatus);

        if (newStatus == JobStatus.Applied)
            await _eventPublisher.PublishApplicationSubmittedAsync(id, userId);

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
