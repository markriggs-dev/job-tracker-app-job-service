using FluentAssertions;
using JobService.Core.Enums;
using JobService.Core.Interfaces;
using JobService.Core.Models;
using JobService.Core.Services;
using Moq;

namespace JobService.UnitTests.Services;

public class JobRequisitionServiceTests
{
    private readonly Mock<IJobRequisitionRepository> _repoMock = new();
    private readonly Mock<IJobEventPublisher> _publisherMock = new();
    private readonly JobRequisitionService _sut;

    public JobRequisitionServiceTests()
    {
        _sut = new JobRequisitionService(_repoMock.Object, _publisherMock.Object);
    }

    private static JobRequisition MakeJob(
        JobStatus status = JobStatus.Discovered,
        DateOnly? dateSubmitted = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = "user-123",
        CompanyName = "Acme Corp",
        RoleTitle = "Senior Engineer",
        Status = status,
        DateDiscovered = DateOnly.FromDateTime(DateTime.UtcNow),
        DateSubmitted = dateSubmitted
    };

    // ── UpdateStatusAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_WhenJobNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                 .ReturnsAsync((JobRequisition?)null);

        var result = await _sut.UpdateStatusAsync(Guid.NewGuid(), "user-123", JobStatus.Applied);

        result.Should().BeNull();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<JobRequisition>()), Times.Never);
    }

    [Theory]
    [InlineData(JobStatus.Applied)]
    [InlineData(JobStatus.InProgress)]
    [InlineData(JobStatus.WaitingOnResponse)]
    [InlineData(JobStatus.InterviewScheduled)]
    [InlineData(JobStatus.OfferReceived)]
    public async Task UpdateStatusAsync_WhenDateSubmittedIsNull_AndStatusImpliesSubmission_SetsDateSubmittedToToday(
        JobStatus newStatus)
    {
        var job = MakeJob(dateSubmitted: null);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _repoMock.Setup(r => r.GetByIdAsync(job.Id, job.UserId)).ReturnsAsync(job);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<JobRequisition>()))
                 .ReturnsAsync((JobRequisition j) => j);

        await _sut.UpdateStatusAsync(job.Id, job.UserId, newStatus);

        _repoMock.Verify(r => r.UpdateAsync(
            It.Is<JobRequisition>(j => j.DateSubmitted == today)), Times.Once);
    }

    [Theory]
    [InlineData(JobStatus.Applied)]
    [InlineData(JobStatus.WaitingOnResponse)]
    [InlineData(JobStatus.InterviewScheduled)]
    public async Task UpdateStatusAsync_WhenDateSubmittedAlreadySet_DoesNotOverwrite(JobStatus newStatus)
    {
        var existingDate = new DateOnly(2026, 1, 15);
        var job = MakeJob(dateSubmitted: existingDate);
        _repoMock.Setup(r => r.GetByIdAsync(job.Id, job.UserId)).ReturnsAsync(job);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<JobRequisition>()))
                 .ReturnsAsync((JobRequisition j) => j);

        await _sut.UpdateStatusAsync(job.Id, job.UserId, newStatus);

        _repoMock.Verify(r => r.UpdateAsync(
            It.Is<JobRequisition>(j => j.DateSubmitted == existingDate)), Times.Once);
    }

    [Theory]
    [InlineData(JobStatus.Discovered)]
    [InlineData(JobStatus.Closed)]
    [InlineData(JobStatus.Withdrawn)]
    public async Task UpdateStatusAsync_WhenStatusDoesNotImplySubmission_LeavesDateSubmittedNull(
        JobStatus newStatus)
    {
        var job = MakeJob(dateSubmitted: null);
        _repoMock.Setup(r => r.GetByIdAsync(job.Id, job.UserId)).ReturnsAsync(job);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<JobRequisition>()))
                 .ReturnsAsync((JobRequisition j) => j);

        await _sut.UpdateStatusAsync(job.Id, job.UserId, newStatus);

        _repoMock.Verify(r => r.UpdateAsync(
            It.Is<JobRequisition>(j => j.DateSubmitted == null)), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_AlwaysSetsStatusAndBumpsUpdatedAt()
    {
        var job = MakeJob();
        var before = job.UpdatedAt;
        _repoMock.Setup(r => r.GetByIdAsync(job.Id, job.UserId)).ReturnsAsync(job);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<JobRequisition>()))
                 .ReturnsAsync((JobRequisition j) => j);

        await _sut.UpdateStatusAsync(job.Id, job.UserId, JobStatus.Applied);

        _repoMock.Verify(r => r.UpdateAsync(It.Is<JobRequisition>(j =>
            j.Status == JobStatus.Applied &&
            j.UpdatedAt >= before)), Times.Once);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenJobNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                 .ReturnsAsync((JobRequisition?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), "user-123");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsMappedResponse()
    {
        var job = MakeJob();
        _repoMock.Setup(r => r.GetByIdAsync(job.Id, job.UserId)).ReturnsAsync(job);

        var result = await _sut.GetByIdAsync(job.Id, job.UserId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(job.Id);
        result.CompanyName.Should().Be("Acme Corp");
        result.RoleTitle.Should().Be("Senior Engineer");
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenJobExists_ReturnsTrueAndCallsRepository()
    {
        var jobId = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteAsync(jobId, "user-123")).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(jobId, "user-123");

        result.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(jobId, "user-123"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenJobNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                 .ReturnsAsync(false);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "user-123");

        result.Should().BeFalse();
    }
}
