using FluentValidation;
using FitnessApp.Modules.Workouts.Application.DTOs;

namespace FitnessApp.Modules.Workouts.Application.Validators;

public class UpdateWorkoutDtoValidator : AbstractValidator<UpdateWorkoutDto>
{
    public UpdateWorkoutDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workout name cannot be empty")
            .MaximumLength(200).WithMessage("Workout name cannot exceed 200 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Difficulty)
            .IsInEnum().WithMessage("Valid difficulty level is required")
            .When(x => x.Difficulty.HasValue);

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Estimated duration must be positive")
            .LessThanOrEqualTo(300).WithMessage("Estimated duration cannot exceed 300 minutes (5 hours)")
            .When(x => x.EstimatedDurationMinutes.HasValue);

        RuleFor(x => x.RequiredEquipment)
            .IsInEnum().WithMessage("Valid equipment type is required")
            .When(x => x.RequiredEquipment.HasValue);

        RuleFor(x => x.ImageContentId)
            .NotEqual(Guid.Empty).WithMessage("Valid image content ID is required")
            .When(x => x.ImageContentId.HasValue);
    }
}

public class UpdateWorkoutPhaseDtoValidator : AbstractValidator<UpdateWorkoutPhaseDto>
{
    public UpdateWorkoutPhaseDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Phase name cannot be empty")
            .MaximumLength(100).WithMessage("Phase name cannot exceed 100 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Phase description cannot exceed 500 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Phase duration must be positive")
            .LessThanOrEqualTo(120).WithMessage("Phase duration cannot exceed 120 minutes")
            .When(x => x.EstimatedDurationMinutes.HasValue);
    }
}

public class AddWorkoutPhaseDtoValidator : AbstractValidator<AddWorkoutPhaseDto>
{
    public AddWorkoutPhaseDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid phase type is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Phase name is required")
            .MaximumLength(100).WithMessage("Phase name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Phase description cannot exceed 500 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Phase duration must be positive")
            .LessThanOrEqualTo(120).WithMessage("Phase duration cannot exceed 120 minutes");
    }
}

public class AddWorkoutExerciseDtoValidator : AbstractValidator<AddWorkoutExerciseDto>
{
    public AddWorkoutExerciseDtoValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEqual(Guid.Empty).WithMessage("Valid exercise ID is required");

        RuleFor(x => x.ExerciseName)
            .NotEmpty().WithMessage("Exercise name is required")
            .MaximumLength(100).WithMessage("Exercise name cannot exceed 100 characters");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps must be positive")
            .LessThanOrEqualTo(1000).WithMessage("Reps cannot exceed 1000")
            .When(x => x.Reps.HasValue);

        RuleFor(x => x.Sets)
            .GreaterThan(0).WithMessage("Sets must be positive")
            .LessThanOrEqualTo(20).WithMessage("Sets cannot exceed 20")
            .When(x => x.Sets.HasValue);

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0).WithMessage("Duration must be positive")
            .LessThanOrEqualTo(7200).WithMessage("Duration cannot exceed 7200 seconds (2 hours)")
            .When(x => x.DurationSeconds.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight cannot be negative")
            .LessThanOrEqualTo(1000).WithMessage("Weight cannot exceed 1000")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.RestTimeSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Rest time cannot be negative")
            .LessThanOrEqualTo(1800).WithMessage("Rest time cannot exceed 1800 seconds (30 minutes)")
            .When(x => x.RestTimeSeconds.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => x.Notes != null);
    }
}

public class UpdateWorkoutExerciseDtoValidator : AbstractValidator<UpdateWorkoutExerciseDto>
{
    public UpdateWorkoutExerciseDtoValidator()
    {
        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps must be positive")
            .LessThanOrEqualTo(1000).WithMessage("Reps cannot exceed 1000")
            .When(x => x.Reps.HasValue);

        RuleFor(x => x.Sets)
            .GreaterThan(0).WithMessage("Sets must be positive")
            .LessThanOrEqualTo(20).WithMessage("Sets cannot exceed 20")
            .When(x => x.Sets.HasValue);

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0).WithMessage("Duration must be positive")
            .LessThanOrEqualTo(7200).WithMessage("Duration cannot exceed 7200 seconds (2 hours)")
            .When(x => x.DurationSeconds.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight cannot be negative")
            .LessThanOrEqualTo(1000).WithMessage("Weight cannot exceed 1000")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.RestTimeSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Rest time cannot be negative")
            .LessThanOrEqualTo(1800).WithMessage("Rest time cannot exceed 1800 seconds (30 minutes)")
            .When(x => x.RestTimeSeconds.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => x.Notes != null);
    }
}

public class WorkoutQueryDtoValidator : AbstractValidator<WorkoutQueryDto>
{
    public WorkoutQueryDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid workout type is required")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Difficulty)
            .IsInEnum().WithMessage("Valid difficulty level is required")
            .When(x => x.Difficulty.HasValue);

        RuleFor(x => x.Equipment)
            .IsInEnum().WithMessage("Valid equipment type is required")
            .When(x => x.Equipment.HasValue);

        RuleFor(x => x.MaxDurationMinutes)
            .GreaterThan(0).WithMessage("Max duration must be positive")
            .LessThanOrEqualTo(300).WithMessage("Max duration cannot exceed 300 minutes")
            .When(x => x.MaxDurationMinutes.HasValue);

        RuleFor(x => x.MinDurationMinutes)
            .GreaterThan(0).WithMessage("Min duration must be positive")
            .LessThanOrEqualTo(300).WithMessage("Min duration cannot exceed 300 minutes")
            .When(x => x.MinDurationMinutes.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => x.SearchTerm != null);

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.CreatedByUserId)
            .NotEqual(Guid.Empty).WithMessage("Valid user ID is required")
            .When(x => x.CreatedByUserId.HasValue);

        RuleFor(x => x.CreatedByCoachId)
            .NotEqual(Guid.Empty).WithMessage("Valid coach ID is required")
            .When(x => x.CreatedByCoachId.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinDurationMinutes.HasValue || !x.MaxDurationMinutes.HasValue || 
                      x.MinDurationMinutes <= x.MaxDurationMinutes)
            .WithMessage("Min duration cannot be greater than max duration");
    }
}
