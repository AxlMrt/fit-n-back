using Microsoft.AspNetCore.Mvc;
using FitnessApp.Modules.Content.Application.Interfaces;
using FitnessApp.Modules.Content.Application.DTOs;

namespace FitnessApp.API.Controllers;

[ApiController]
[Route("api/content/assets")]
public class MediaAssetsController : ControllerBase
{
    private readonly IMediaAssetService _service;
    private readonly IMediaAssetRepository _repository;

    public MediaAssetsController(IMediaAssetService service, IMediaAssetRepository repository)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] Guid exerciseId, [FromForm] string? description)
    {
        if (file == null || file.Length == 0) return BadRequest("No file provided");

        await using var stream = file.OpenReadStream();
        var id = await _service.UploadAsync(stream, file.FileName, file.ContentType ?? "application/octet-stream", exerciseId, description);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("by-exercise/{exerciseId:guid}")]
    public async Task<IActionResult> GetByExercise(Guid exerciseId)
    {
        var assets = await _service.GetByExerciseIdAsync(exerciseId);
        var list = assets.Select(a => new MediaAssetResponse(a.Id, a.Url, a.Type, a.Description, a.CreatedAt, a.ContentType));
        return Ok(list);
    }

    [HttpGet("{id:guid}", Name = "GetById")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null) return NotFound();
        var dto = new MediaAssetResponse(asset.Id, asset.Url, asset.Type, asset.Description, asset.CreatedAt, asset.ContentType);
        return Ok(dto);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null) return NotFound();
        var stream = await _service.DownloadAsync(id);
        return File(stream, asset.ContentType ?? "application/octet-stream", Path.GetFileName(asset.Key));
    }

    [HttpGet("exercises/{exerciseId:guid}/assets")]
    public async Task<IActionResult> ListByExercise(Guid exerciseId)
    {
        var assets = await _service.GetByExerciseIdAsync(exerciseId);
        var list = assets.Select(a => new MediaAssetResponse(a.Id, a.Url, a.Type, a.Description, a.CreatedAt, a.ContentType));
        return Ok(list);
    }
}
