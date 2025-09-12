using Microsoft.AspNetCore.Mvc;
using FitnessApp.Modules.Content.Application.Interfaces;
using FitnessApp.Modules.Content.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using FitnessApp.Modules.Authorization.Policies;
using FitnessApp.SharedKernel.DTOs.Requests;

namespace FitnessApp.API.Controllers.v1;

[ApiController]
[Authorize]
[Route("api/v1/content/assets")]
[Produces("application/json")]
public class MediaAssetsController : ControllerBase
{
    private readonly IMediaAssetService _service;
    private readonly IMediaAssetRepository _repository;

    public MediaAssetsController(IMediaAssetService service, IMediaAssetRepository repository)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Upload a media asset for an exercise.
    /// </summary>
    /// <remarks>
    /// Accepts multipart/form-data. Returns the created asset id and Location header.
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    public async Task<IActionResult> Upload([FromForm] MediaAssetUploadRequest request)
    {
        if (request.File == null || request.File.Length == 0) return BadRequest("No file provided");

        await using var stream = request.File.OpenReadStream();
        var id = await _service.UploadAsync(stream, request.File.FileName, request.File.ContentType ?? "application/octet-stream", request.ExerciseId, request.Description);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Get media assets for a given exercise.
    /// </summary>
    [HttpGet("by-exercise/{exerciseId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<MediaAssetResponse>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> GetByExercise(Guid exerciseId)
    {
        var assets = await _service.GetByExerciseIdAsync(exerciseId);
        var list = assets.Select(a => new MediaAssetResponse(a.Id, a.Url, a.Type, a.Description, a.CreatedAt, a.ContentType));
        return Ok(list);
    }

    /// <summary>
    /// Get media asset by id.
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetById")]
    [ProducesResponseType(typeof(MediaAssetResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null) return NotFound();
        var dto = new MediaAssetResponse(asset.Id, asset.Url, asset.Type, asset.Description, asset.CreatedAt, asset.ContentType);
        return Ok(dto);
    }

    /// <summary>
    /// Download a media asset file.
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> Download(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null) return NotFound();
        var stream = await _service.DownloadAsync(id);
        return File(stream, asset.ContentType ?? "application/octet-stream", Path.GetFileName(asset.Key));
    }

    /// <summary>
    /// Delete a media asset.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// List assets by exercise (alias of GetByExercise).
    /// </summary>
    [HttpGet("exercises/{exerciseId:guid}/assets")]
    [ProducesResponseType(typeof(IEnumerable<MediaAssetResponse>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> ListByExercise(Guid exerciseId)
    {
        var assets = await _service.GetByExerciseIdAsync(exerciseId);
        var list = assets.Select(a => new MediaAssetResponse(a.Id, a.Url, a.Type, a.Description, a.CreatedAt, a.ContentType));
        return Ok(list);
    }
}