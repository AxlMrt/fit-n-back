using Microsoft.AspNetCore.Mvc;
using FitnessApp.Modules.Authorization.Policies;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using FitnessApp.Modules.Exercises.Domain.Exceptions;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.API.Infrastructure.Errors;
using FitnessApp.API.Controllers;

namespace FitnessApp.API.Controllers.v1;

[Authorize]
[Route("api/v1/exercises")]
public class ExercisesController : BaseController
{

    private readonly IExerciseService _exerciseService;
    private readonly ILogger<ExercisesController> _logger;

    public ExercisesController(IExerciseService exerciseService, ILogger<ExercisesController> logger)
    {
        _exerciseService = exerciseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all exercises with optional filtering and pagination.
    /// </summary>
    /// <remarks>
    /// Returns a paged list of exercises. Query supports paging, filtering and sorting.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ExerciseListDto>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<PagedResult<ExerciseListDto>>> GetExercises([FromQuery] ExerciseQueryDto query)
    {
        var result = await _exerciseService.GetPagedAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Get exercise by ID.
    /// </summary>
    /// <param name="id">Exercise identifier (GUID).</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExerciseDto), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ExerciseDto>> GetExercise(Guid id)
    {
        var exercise = await _exerciseService.GetByIdAsync(id);
        if (exercise == null)
            return NotFound(new { message = $"Exercise with ID {id} not found" });

        return Ok(exercise);
    }

    /// <summary>
    /// Search exercises by name.
    /// </summary>
    /// <param name="term">Search term (part of name).</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ExerciseListDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<ActionResult<IEnumerable<ExerciseListDto>>> SearchExercises([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest(new { message = "Search term is required" });

        var exercises = await _exerciseService.SearchAsync(term);
        return Ok(exercises);
    }

    /// <summary>
    /// Create a new exercise.
    /// </summary>
    /// <remarks>
    /// Requires Admin role. Returns the created exercise with 201 Created and Location header.
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ExerciseDto), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ExerciseDto>> CreateExercise([FromBody] CreateExerciseDto createDto)
    {
        var exercise = await _exerciseService.CreateAsync(createDto);
        return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, exercise);
    }

    /// <summary>
    /// Update an existing exercise.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ExerciseDto), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ExerciseDto>> UpdateExercise(Guid id, [FromBody] UpdateExerciseDto updateDto)
    {
        var exercise = await _exerciseService.UpdateAsync(id, updateDto);
        if (exercise == null)
            return NotFound(new { message = $"Exercise with ID {id} not found" });

        return Ok(exercise);
    }

    /// <summary>
    /// Delete an exercise.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult> DeleteExercise(Guid id)
    {
        var success = await _exerciseService.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = $"Exercise with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Activate an exercise.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult> ActivateExercise(Guid id)
    {
        var success = await _exerciseService.ActivateAsync(id);
        if (!success)
            return NotFound(new { message = $"Exercise with ID {id} not found" });

        return Ok(new { message = "Exercise activated successfully" });
    }

    /// <summary>
    /// Deactivate an exercise.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult> DeactivateExercise(Guid id)
    {
        var success = await _exerciseService.DeactivateAsync(id);
        if (!success)
            return NotFound(new { message = $"Exercise with ID {id} not found" });

        return Ok(new { message = "Exercise deactivated successfully" });
    }

    /// <summary>
    /// Check if exercise exists.
    /// </summary>
    [HttpHead("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> ExerciseExists(Guid id)
    {
        var exists = await _exerciseService.ExistsAsync(id);
        return exists ? Ok() : NotFound();
    }
}
