using LogService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "AdminOnly")]  // Log sorgulaması yalnızca Admin'e açık
[Produces("application/json")]
public class LogsController : ControllerBase
{
    private readonly ILogRepository _repo;

    public LogsController(ILogRepository repo) => _repo = repo;

    /// <summary>Servise göre logları listeler.</summary>
    [HttpGet("by-service/{serviceName}")]
    public async Task<IActionResult> GetByService(
        string serviceName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var logs = await _repo.GetByServiceAsync(serviceName, page, pageSize, ct);
        return Ok(logs);
    }

    /// <summary>Seviyeye göre logları listeler (INFO, WARNING, ERROR, CRITICAL).</summary>
    [HttpGet("by-level/{level}")]
    public async Task<IActionResult> GetByLevel(
        string level,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var logs = await _repo.GetByLevelAsync(level, page, pageSize, ct);
        return Ok(logs);
    }

    /// <summary>Correlation ID ile dağıtık trace sorgular.</summary>
    [HttpGet("by-correlation/{correlationId}")]
    public async Task<IActionResult> GetByCorrelation(
        string correlationId,
        CancellationToken ct)
    {
        var logs = await _repo.GetByCorrelationIdAsync(correlationId, ct);
        return Ok(logs);
    }
}
