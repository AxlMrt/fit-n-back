using Microsoft.AspNetCore.Mvc;
using FitnessApp.Modules.Authorization.Policies;
using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using FitnessApp.Modules.Exercises.Domain.Exceptions;

namespace FitnessApp.API.Controllers.v1;

[ApiController]
[Authorize]
[Route("api/v1/exercises")]
public class ExercisesController : ControllerBase
{

    private readonly IExerciseService _exerciseService;
    private readonly ILogger<ExercisesController> _logger;

    public ExercisesController(IExerciseService exerciseService, ILogger<ExercisesController> logger)
    {
        _exerciseService = exerciseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all exercises with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ExerciseListDto>>> GetExercises([FromQuery] ExerciseQueryDto query)
    {
        try
        {
            var result = await _exerciseService.GetPagedAsync(query);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    /// <summary>
    /// Get exercise by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExerciseDto>> GetExercise(Guid id)
    {
        try
        {
            var exercise = await _exerciseService.GetByIdAsync(id);
            if (exercise == null)
                return NotFound(new { message = $"Exercise with ID {id} not found" });

            return Ok(exercise);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Search exercises by name
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ExerciseListDto>>> SearchExercises([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest(new { message = "Search term is required" });

        var exercises = await _exerciseService.SearchAsync(term);
        return Ok(exercises);
    }

    /// <summary>
    /// Create a new exercise
    /// </summary>
    [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
        public async Task<ActionResult<ExerciseDto>> CreateExercise([FromBody] CreateExerciseDto createDto)
    {
        try
        {
            var exercise = await _exerciseService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, exercise);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (ExerciseDomainException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

        /// <summary>
        /// Update an existing exercise
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ExerciseDto>> UpdateExercise(Guid id, [FromBody] UpdateExerciseDto updateDto)
        {
            try
            {
                var exercise = await _exerciseService.UpdateAsync(id, updateDto);
                if (exercise == null)
                    return NotFound(new { message = $"Exercise with ID {id} not found" });

                return Ok(exercise);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (ExerciseDomainException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

    /// <summary>
    /// Delete an exercise
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
        
        public async Task<ActionResult> DeleteExercise(Guid id)
    {
        var success = await _exerciseService.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = $"Exercise with ID {id} not found" });

        return NoContent();
    }

        /// <summary>
        /// Activate an exercise
        /// </summary>
        [HttpPost("{id:guid}/activate")]
        public async Task<ActionResult> ActivateExercise(Guid id)
        {
            var success = await _exerciseService.ActivateAsync(id);
            if (!success)
                return NotFound(new { message = $"Exercise with ID {id} not found" });

            return Ok(new { message = "Exercise activated successfully" });
        }

        /// <summary>
        /// Deactivate an exercise
        /// </summary>
        [HttpPost("{id:guid}/deactivate")]
        public async Task<ActionResult> DeactivateExercise(Guid id)
        {
            var success = await _exerciseService.DeactivateAsync(id);
            if (!success)
                return NotFound(new { message = $"Exercise with ID {id} not found" });

            return Ok(new { message = "Exercise deactivated successfully" });
        }

        /// <summary>
        /// Check if exercise exists
        /// </summary>
        [HttpHead("{id:guid}")]
        public async Task<ActionResult> ExerciseExists(Guid id)
        {
            var exists = await _exerciseService.ExistsAsync(id);
            return exists ? Ok() : NotFound();
        }
}
