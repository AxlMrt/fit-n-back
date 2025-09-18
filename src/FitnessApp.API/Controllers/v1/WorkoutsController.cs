using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FitnessApp.Modules.Workouts.Application.Interfaces;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;
using System.Security.Claims;

namespace FitnessApp.API.Controllers.v1;

/// <summary>
/// API controller for workout operations
/// </summary>
[ApiController]
[Route("api/v1/workouts")]
[Produces("application/json")]
public class WorkoutsController : ControllerBase
{
    #region Fields

    private readonly IWorkoutService _workoutService;

    #endregion

    #region Constructor

    public WorkoutsController(IWorkoutService workoutService)
    {
        _workoutService = workoutService ?? throw new ArgumentNullException(nameof(workoutService));
    }

    #endregion

    #region Basic CRUD Operations

    /// <summary>
    /// Create a new workout.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkoutDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> CreateWorkout([FromBody] CreateWorkoutDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.CreateWorkoutAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Get workout by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutService.GetWorkoutByIdAsync(id, cancellationToken);

        if (workout == null)
            return NotFound(new { Message = $"Workout with ID {id} not found" });

        return Ok(workout);
    }

    /// <summary>
    /// Update an existing workout.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> UpdateWorkout(Guid id, [FromBody] UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.UpdateWorkoutAsync(id, updateDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Delete a workout.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> DeleteWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _workoutService.DeleteWorkoutAsync(id, cancellationToken);

        if (!deleted)
            return NotFound(new { Message = $"Workout with ID {id} not found" });

        return NoContent();
    }

    #endregion

    #region Query Operations

    /// <summary>
    /// Get workouts with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> GetWorkouts([FromQuery] WorkoutQueryDto query, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _workoutService.GetPagedWorkoutsAsync(query.PageNumber, query.PageSize, query.Type, query.Category, query.Difficulty, query.NameFilter, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", ex.Errors });
        }
    }

    /// <summary>
    /// Get active workouts only.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetActiveWorkouts(CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetActiveWorkoutsAsync(cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get all template workouts (workouts available to all users).
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetTemplateWorkouts(CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetTemplateWorkoutsAsync(cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get paginated workouts with optional filtering.
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetPagedWorkouts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] WorkoutType? type = null,
        [FromQuery] WorkoutCategory? category = null,
        [FromQuery] DifficultyLevel? difficulty = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _workoutService.GetPagedWorkoutsAsync(page, pageSize, type, category, difficulty, searchTerm, cancellationToken);

        var response = new
        {
            Data = result.Workouts,
            Pagination = new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = result.TotalCount,
                TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize)
            }
        };

        return Ok(response);
    }

    #endregion

    #region Category Operations

    /// <summary>
    /// Get workouts by category.
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetWorkoutsByCategory(WorkoutCategory category, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetWorkoutsByCategoryAsync(category, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get workouts by category and difficulty.
    /// </summary>
    [HttpGet("category/{category}/difficulty/{difficulty}")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetWorkoutsByCategoryAndDifficulty(WorkoutCategory category, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetWorkoutsByCategoryAndDifficultyAsync(category, difficulty, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get workouts by type.
    /// </summary>
    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetWorkoutsByType(WorkoutType type, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetWorkoutsByTypeAsync(type, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get workouts by difficulty level.
    /// </summary>
    [HttpGet("difficulty/{difficulty}")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetWorkoutsByDifficulty(DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetWorkoutsByDifficultyAsync(difficulty, cancellationToken);
        return Ok(workouts);
    }

    #endregion

    #region Search Operations

    /// <summary>
    /// Search workouts by name or description, optionally filtered by category.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> SearchWorkouts([FromQuery] string searchTerm, [FromQuery] WorkoutCategory? category = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(new { Message = "Search term is required" });
        }

        var workouts = await _workoutService.SearchWorkoutsAsync(searchTerm, category, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get workouts with advanced filtering options.
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetWorkoutsWithFilters(
        [FromQuery] WorkoutType? type = null,
        [FromQuery] WorkoutCategory? category = null,
        [FromQuery] DifficultyLevel? difficulty = null,
        [FromQuery] int? minDuration = null,
        [FromQuery] int? maxDuration = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetWorkoutsWithAdvancedFiltersAsync(type, category, difficulty, minDuration, maxDuration, includeInactive, cancellationToken);
        return Ok(workouts);
    }

    #endregion

    #region User-Specific Operations

    /// <summary>
    /// Get workouts created by a specific user.
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetUserWorkouts(Guid userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetUserWorkoutsAsync(userId, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get workouts created by a specific coach.
    /// </summary>
    [HttpGet("coach/{coachId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    public async Task<IActionResult> GetCoachWorkouts(Guid coachId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetCoachWorkoutsAsync(coachId, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get workouts created by the current authenticated user.
    /// This endpoint would typically extract the user ID from the authentication context.
    /// For now, it requires the userId as a query parameter.
    /// </summary>
    [HttpGet("my-workouts")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> GetMyWorkouts(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
        {
            return BadRequest(new { Message = "User ID is required" });
        }

        var workouts = await _workoutService.GetUserWorkoutsAsync(userId, cancellationToken);
        return Ok(workouts);
    }

    #endregion

    #region Workout Management

    /// <summary>
    /// Duplicate an existing workout.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType(typeof(WorkoutDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> DuplicateWorkout(Guid id, [FromBody] DuplicateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.DuplicateWorkoutAsync(id, request.NewName, cancellationToken);
            return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a workout.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> DeactivateWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        var deactivated = await _workoutService.DeactivateWorkoutAsync(id, cancellationToken);

        if (!deactivated)
            return NotFound(new { Message = $"Workout with ID {id} not found" });

        return Ok(new { Message = "Workout deactivated successfully" });
    }

    /// <summary>
    /// Reactivate a workout.
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> ReactivateWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        var reactivated = await _workoutService.ReactivateWorkoutAsync(id, cancellationToken);

        if (!reactivated)
            return NotFound(new { Message = $"Workout with ID {id} not found" });

        return Ok(new { Message = "Workout reactivated successfully" });
    }

    #endregion

    #region Utility Operations

    /// <summary>
    /// Check if a workout exists.
    /// </summary>
    [HttpHead("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> WorkoutExists(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _workoutService.ExistsAsync(id, cancellationToken);
        return exists ? Ok() : NotFound();
    }

    /// <summary>
    /// Get total workout count with optional filtering.
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetWorkoutCount([FromQuery] WorkoutQueryDto? query = null, CancellationToken cancellationToken = default)
    {
        var count = await _workoutService.CountAsync(cancellationToken);
        return Ok(new { Count = count });
    }

    #endregion

    #region Workout Phases

    /// <summary>
    /// Add a phase to a workout.
    /// </summary>
    [HttpPost("{workoutId:guid}/phases")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> AddPhaseToWorkout(Guid workoutId, [FromBody] AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.AddPhaseToWorkoutAsync(workoutId, phaseDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Update a workout phase.
    /// </summary>
    [HttpPut("{workoutId:guid}/phases/{phaseId:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> UpdateWorkoutPhase(Guid workoutId, Guid phaseId, [FromBody] UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.UpdateWorkoutPhaseAsync(workoutId, phaseId, updateDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Remove a phase from a workout.
    /// </summary>
    [HttpDelete("{workoutId:guid}/phases/{phaseId:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> RemovePhaseFromWorkout(Guid workoutId, Guid phaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.RemovePhaseFromWorkoutAsync(workoutId, phaseId, cancellationToken);
            return Ok(workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Move a workout phase to a new order position.
    /// </summary>
    [HttpPut("{workoutId:guid}/phases/{phaseId:guid}/move")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> MoveWorkoutPhase(Guid workoutId, Guid phaseId, [FromBody] MovePhaseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.MoveWorkoutPhaseAsync(workoutId, phaseId, request.NewOrder, cancellationToken);
            return Ok(workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    #endregion

    #region Phase Exercises

    /// <summary>
    /// Add an exercise to a workout phase.
    /// </summary>
    [HttpPost("{workoutId:guid}/phases/{phaseId:guid}/exercises")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> AddExerciseToPhase(Guid workoutId, Guid phaseId, [FromBody] AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.AddExerciseToPhaseAsync(workoutId, phaseId, exerciseDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Update an exercise in a workout phase.
    /// </summary>
    [HttpPut("{workoutId:guid}/phases/{phaseId:guid}/exercises/{exerciseId:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> UpdatePhaseExercise(Guid workoutId, Guid phaseId, Guid exerciseId, [FromBody] UpdateWorkoutExerciseDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.UpdatePhaseExerciseAsync(workoutId, phaseId, exerciseId, updateDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Remove an exercise from a workout phase.
    /// </summary>
    [HttpDelete("{workoutId:guid}/phases/{phaseId:guid}/exercises/{exerciseId:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> RemoveExerciseFromPhase(Guid workoutId, Guid phaseId, Guid exerciseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.RemoveExerciseFromPhaseAsync(workoutId, phaseId, exerciseId, cancellationToken);
            return Ok(workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Move an exercise within a phase to a new order position.
    /// </summary>
    [HttpPut("{workoutId:guid}/phases/{phaseId:guid}/exercises/{exerciseId:guid}/move")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> MovePhaseExercise(Guid workoutId, Guid phaseId, Guid exerciseId, [FromBody] MoveExerciseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.MovePhaseExerciseAsync(workoutId, phaseId, exerciseId, request.NewOrder, cancellationToken);
            return Ok(workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    #endregion

    #region Utility Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }

    #endregion
}

// Request DTOs for controller actions
public record DuplicateWorkoutRequest(string NewName);
public record MovePhaseRequest(int NewOrder);
public record MoveExerciseRequest(int NewOrder);
