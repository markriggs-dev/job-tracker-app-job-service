using JobService.Core.DTOs;
using JobService.Core.Enums;
using JobService.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobService.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobRequisitionsController : ControllerBase
{
    private readonly JobRequisitionService _service;
    private readonly ILogger<JobRequisitionsController> _logger;

    public JobRequisitionsController(
        JobRequisitionService service,
        ILogger<JobRequisitionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User ID not found in token");

    private string? GetUserEmail() =>
        User.FindFirstValue("https://job-tracker/email")
        ?? User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email");

    // GET /api/jobs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobRequisitionListResponse>>> GetAll()
    {
        var userId = GetUserId();
        var results = await _service.GetAllAsync(userId);
        return Ok(results);
    }

    // GET /api/jobs/search?keyword=boeing&status=Applied
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<JobRequisitionListResponse>>> Search(
        [FromQuery] string? keyword,
        [FromQuery] JobStatus? status)
    {
        var userId = GetUserId();
        var results = await _service.SearchAsync(userId, keyword, status);
        return Ok(results);
    }

    // GET /api/jobs/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobRequisitionResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        var result = await _service.GetByIdAsync(id, userId);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    // POST /api/jobs — returns 202 Accepted; consumer writes to DB
    [HttpPost]
    public async Task<ActionResult<JobRequisitionAcceptedResponse>> Create(
        [FromBody] CreateJobRequisitionRequest request)
    {
        var userId = GetUserId();
        var result = await _service.CreateAsync(userId, GetUserEmail(), request);

        return Accepted(result);
    }

    // PUT /api/jobs/{id} — returns 202 Accepted; consumer writes to DB
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobRequisitionAcceptedResponse>> Update(
        Guid id,
        [FromBody] UpdateJobRequisitionRequest request)
    {
        var userId = GetUserId();
        var result = await _service.UpdateAsync(id, userId, GetUserEmail(), request);

        if (result is null)
            return NotFound();

        return Accepted(result);
    }

    // PATCH /api/jobs/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<JobRequisitionResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateJobStatusRequest request)
    {
        var userId = GetUserId();
        var result = await _service.UpdateStatusAsync(id, userId, request.Status);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    // DELETE /api/jobs/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await _service.DeleteAsync(id, userId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
