using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FitnessApp.Modules.Workouts.Application.Interfaces;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;
using FitnessApp.Modules.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FitnessApp.API.Controllers.v1;

/// <summary>
/// API controller for workout operations with user/admin role separation
/// </summary>
[ApiController]
[Authorize]
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

    #region Public Read Operations (All Users)

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

    #endregion

    #region User Operations (UserCreated workouts only)

    /// <summary>
    /// Create a new user workout.
    /// </summary>
    [HttpPost("my-workouts")]
    [ProducesResponseType(typeof(WorkoutDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> CreateUserWorkout([FromBody] CreateWorkoutDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var workout = await _workoutService.CreateUserWorkoutAsync(createDto, userId, cancellationToken);
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
    /// Get workouts created by the current authenticated user.
    /// </summary>
    [HttpGet("my-workouts")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutListDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> GetMyWorkouts(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var workouts = await _workoutService.GetUserWorkoutsAsync(userId, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Update a user's own workout.
    /// </summary>
    [HttpPut("my-workouts/{id:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> UpdateUserWorkout(Guid id, [FromBody] UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var workout = await _workoutService.UpdateUserWorkoutAsync(id, updateDto, userId, cancellationToken);
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
    /// Delete a user's own workout.
    /// </summary>
    [HttpDelete("my-workouts/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> DeleteUserWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deleted = await _workoutService.DeleteUserWorkoutAsync(id, userId, cancellationToken);

            if (!deleted)
                return NotFound(new { Message = $"Workout with ID {id} not found or not owned by user" });

            return NoContent();
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Duplicate a user's own workout.
    /// </summary>
    [HttpPost("my-workouts/{id:guid}/duplicate")]
    [ProducesResponseType(typeof(WorkoutDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> DuplicateUserWorkout(Guid id, [FromBody] DuplicateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var workout = await _workoutService.DuplicateUserWorkoutAsync(id, request.NewName, userId, cancellationToken);
            return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    #region User Phase Management

    /// <summary>
    /// Add a phase to user's workout.
    /// </summary>
    [HttpPost("my-workouts/{workoutId:guid}/phases")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> AddPhaseToUserWorkout(Guid workoutId, [FromBody] AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var workout = await _workoutService.AddPhaseToUserWorkoutAsync(workoutId, phaseDto, userId, cancellationToken);
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
    /// Update a phase in user's workout.
    /// </summary>
    [HttpPut("my-workouts/{workoutId:guid}/phases/{phaseId:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> UpdateUserWorkoutPhase(Guid workoutId, Guid phaseId, [FromBody] UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var workout = await _workoutService.UpdateUserWorkoutPhaseAsync(workoutId, phaseId, updateDto, userId, cancellationToken);
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
    /// Add an exercise to a phase in user's workout.
    /// </summary>
    [HttpPost("my-workouts/{workoutId:guid}/phases/{phaseId:guid}/exercises")]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> AddExerciseToUserPhase(Guid workoutId, Guid phaseId, [FromBody] AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var workout = await _workoutService.AddExerciseToUserPhaseAsync(workoutId, phaseId, exerciseDto, userId, cancellationToken);
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

    #endregion

    #endregion

    #region Admin Operations (Template workouts and all workouts)

    /// <summary>
    /// Admin: Create a new template workout.
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(WorkoutDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    public async Task<IActionResult> CreateTemplateWorkout([FromBody] CreateWorkoutDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.CreateTemplateWorkoutAsync(createDto, cancellationToken);
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
    /// Admin: Update any workout.
    /// </summary>
    [HttpPut("admin/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> UpdateWorkoutAsAdmin(Guid id, [FromBody] UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.UpdateWorkoutAsAdminAsync(id, updateDto, cancellationToken);
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
    /// Admin: Delete any workout.
    /// </summary>
    [HttpDelete("admin/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> DeleteWorkoutAsAdmin(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _workoutService.DeleteWorkoutAsAdminAsync(id, cancellationToken);

        if (!deleted)
            return NotFound(new { Message = $"Workout with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Admin: Get all workouts with pagination and filtering.
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetAllWorkoutsAsAdmin(
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

    /// <summary>
    /// Admin: Add a phase to any workout.
    /// </summary>
    [HttpPost("admin/{workoutId:guid}/phases")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(WorkoutDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> AddPhaseToWorkoutAsAdmin(Guid workoutId, [FromBody] AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.AddPhaseToWorkoutAsAdminAsync(workoutId, phaseDto, cancellationToken);
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
