using JobService.Core.Enums;

namespace JobService.Core.DTOs;

public record CreateJobRequisitionRequest(
    string CompanyName,
    string RoleTitle,
    string? SourceUrl,
    string? CompanyCareerPortalUrl,
    string? JobDescription,
    DateOnly DateDiscovered,
    DateOnly? ApplicationExpiryDate
);

public record UpdateJobRequisitionRequest(
    string CompanyName,
    string RoleTitle,
    string? SourceUrl,
    string? CompanyCareerPortalUrl,
    string? JobDescription,
    DateOnly DateDiscovered,
    DateOnly? ApplicationExpiryDate,
    DateOnly? DateSubmitted
);

public record UpdateJobStatusRequest(
    JobStatus Status
);

public record JobRequisitionResponse(
    Guid Id,
    string CompanyName,
    string RoleTitle,
    string? SourceUrl,
    string? CompanyCareerPortalUrl,
    string? JobDescription,
    JobStatus Status,
    string StatusDisplay,
    DateOnly DateDiscovered,
    DateOnly? ApplicationExpiryDate,
    DateOnly? DateSubmitted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record JobRequisitionListResponse(
    Guid Id,
    string CompanyName,
    string RoleTitle,
    JobStatus Status,
    string StatusDisplay,
    DateOnly DateDiscovered,
    DateOnly? DateSubmitted,
    DateOnly? ApplicationExpiryDate
);
