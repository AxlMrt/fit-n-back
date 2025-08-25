using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FitnessApp.Modules.Workouts.Application.Interfaces;
using FitnessApp.Modules.Workouts.Application.DTOs;
using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.API.Controllers;

/// <summary>
/// API controller for workout operations
/// </summary>
[ApiController]
[Route("api/v1/workouts")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;

    public WorkoutsController(IWorkoutService workoutService)
    {
        _workoutService = workoutService ?? throw new ArgumentNullException(nameof(workoutService));
    }

    /// <summary>
    /// Create a new workout
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorkout([FromBody] CreateWorkoutDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.CreateWorkoutAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", Errors = ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Get workout by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutService.GetWorkoutByIdAsync(id, cancellationToken);
        
        if (workout == null)
            return NotFound(new { Message = $"Workout with ID {id} not found" });
            
        return Ok(workout);
    }

    /// <summary>
    /// Update an existing workout
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkout(Guid id, [FromBody] UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.UpdateWorkoutAsync(id, updateDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", Errors = ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a workout
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _workoutService.DeleteWorkoutAsync(id, cancellationToken);
        
        if (!deleted)
            return NotFound(new { Message = $"Workout with ID {id} not found" });
            
        return NoContent();
    }

    /// <summary>
    /// Get workouts with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkouts([FromQuery] WorkoutQueryDto query, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _workoutService.GetWorkoutsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", Errors = ex.Errors });
        }
    }

    /// <summary>
    /// Get workouts created by a specific user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserWorkouts(Guid userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetUserWorkoutsAsync(userId, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get workouts created by a specific coach
    /// </summary>
    [HttpGet("coach/{coachId:guid}")]
    public async Task<IActionResult> GetCoachWorkouts(Guid coachId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutService.GetCoachWorkoutsAsync(coachId, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Duplicate an existing workout
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateWorkout(Guid id, [FromBody] DuplicateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.DuplicateWorkoutAsync(id, request.NewName, cancellationToken);
            return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a workout
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        var deactivated = await _workoutService.DeactivateWorkoutAsync(id, cancellationToken);
        
        if (!deactivated)
            return NotFound(new { Message = $"Workout with ID {id} not found" });
            
        return Ok(new { Message = "Workout deactivated successfully" });
    }

    /// <summary>
    /// Reactivate a workout
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> ReactivateWorkout(Guid id, CancellationToken cancellationToken = default)
    {
        var reactivated = await _workoutService.ReactivateWorkoutAsync(id, cancellationToken);
        
        if (!reactivated)
            return NotFound(new { Message = $"Workout with ID {id} not found" });
            
        return Ok(new { Message = "Workout reactivated successfully" });
    }

    #region Workout Phases

    /// <summary>
    /// Add a phase to a workout
    /// </summary>
    [HttpPost("{workoutId:guid}/phases")]
    public async Task<IActionResult> AddPhaseToWorkout(Guid workoutId, [FromBody] AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.AddPhaseToWorkoutAsync(workoutId, phaseDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", Errors = ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Update a workout phase
    /// </summary>
    [HttpPut("{workoutId:guid}/phases/{phaseId:guid}")]
    public async Task<IActionResult> UpdateWorkoutPhase(Guid workoutId, Guid phaseId, [FromBody] UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.UpdateWorkoutPhaseAsync(workoutId, phaseId, updateDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", Errors = ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a phase from a workout
    /// </summary>
    [HttpDelete("{workoutId:guid}/phases/{phaseId:guid}")]
    public async Task<IActionResult> RemovePhaseFromWorkout(Guid workoutId, Guid phaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.RemovePhaseFromWorkoutAsync(workoutId, phaseId, cancellationToken);
            return Ok(workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Move a workout phase to a new order position
    /// </summary>
    [HttpPut("{workoutId:guid}/phases/{phaseId:guid}/move")]
    public async Task<IActionResult> MoveWorkoutPhase(Guid workoutId, Guid phaseId, [FromBody] MovePhaseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.MoveWorkoutPhaseAsync(workoutId, phaseId, request.NewOrder, cancellationToken);
            return Ok(workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    #endregion

    #region Phase Exercises

    /// <summary>
    /// Add an exercise to a workout phase
    /// </summary>
    [HttpPost("{workoutId:guid}/phases/{phaseId:guid}/exercises")]
    public async Task<IActionResult> AddExerciseToPhase(Guid workoutId, Guid phaseId, [FromBody] AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.AddExerciseToPhaseAsync(workoutId, phaseId, exerciseDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", Errors = ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Update an exercise in a workout phase
    /// </summary>
    [HttpPut("{workoutId:guid}/phases/{phaseId:guid}/exercises/{exerciseId:guid}")]
    public async Task<IActionResult> UpdatePhaseExercise(Guid workoutId, Guid phaseId, Guid exerciseId, [FromBody] UpdateWorkoutExerciseDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.UpdatePhaseExerciseAsync(workoutId, phaseId, exerciseId, updateDto, cancellationToken);
            return Ok(workout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { Message = "Validation failed", Errors = ex.Errors });
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Remove an exercise from a workout phase
    /// </summary>
    [HttpDelete("{workoutId:guid}/phases/{phaseId:guid}/exercises/{exerciseId:guid}")]
    public async Task<IActionResult> RemoveExerciseFromPhase(Guid workoutId, Guid phaseId, Guid exerciseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.RemoveExerciseFromPhaseAsync(workoutId, phaseId, exerciseId, cancellationToken);
            return Ok(workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Move an exercise within a phase to a new order position
    /// </summary>
    [HttpPut("{workoutId:guid}/phases/{phaseId:guid}/exercises/{exerciseId:guid}/move")]
    public async Task<IActionResult> MovePhaseExercise(Guid workoutId, Guid phaseId, Guid exerciseId, [FromBody] MoveExerciseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var workout = await _workoutService.MovePhaseExerciseAsync(workoutId, phaseId, exerciseId, request.NewOrder, cancellationToken);
            return Ok(workout);
        }
        catch (WorkoutDomainException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    #endregion
}

// Request DTOs for controller actions
public record DuplicateWorkoutRequest(string NewName);
public record MovePhaseRequest(int NewOrder);
public record MoveExerciseRequest(int NewOrder);
