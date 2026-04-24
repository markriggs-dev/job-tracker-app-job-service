using JobService.Core.Enums;

namespace JobService.Core.Models;

public class JobRequisition
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string? CompanyCareerPortalUrl { get; set; }
    public string? JobDescription { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Discovered;
    public DateOnly DateDiscovered { get; set; }
    public DateOnly? ApplicationExpiryDate { get; set; }
    public DateOnly? DateSubmitted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
