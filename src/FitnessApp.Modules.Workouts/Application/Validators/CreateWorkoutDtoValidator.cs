using FluentValidation;
using FitnessApp.Modules.Workouts.Application.DTOs;
using FitnessApp.Modules.Workouts.Domain.Enums;

namespace FitnessApp.Modules.Workouts.Application.Validators;

public class CreateWorkoutDtoValidator : AbstractValidator<CreateWorkoutDto>
{
    public CreateWorkoutDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workout name is required")
            .MaximumLength(200).WithMessage("Workout name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Valid workout type is required");

        RuleFor(x => x.Difficulty)
            .IsInEnum().WithMessage("Valid difficulty level is required");

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Estimated duration must be positive")
            .LessThanOrEqualTo(300).WithMessage("Estimated duration cannot exceed 300 minutes (5 hours)");

        RuleFor(x => x.RequiredEquipment)
            .IsInEnum().WithMessage("Valid equipment type is required");

        RuleFor(x => x.CreatedByUserId)
            .NotEqual(Guid.Empty).WithMessage("Valid user ID is required")
            .When(x => x.CreatedByUserId.HasValue);

        RuleFor(x => x.CreatedByCoachId)
            .NotEqual(Guid.Empty).WithMessage("Valid coach ID is required")
            .When(x => x.CreatedByCoachId.HasValue);

        RuleFor(x => x.Phases)
            .Must(phases => phases == null || phases.Count <= 10)
            .WithMessage("Maximum 10 phases are allowed per workout");

        RuleForEach(x => x.Phases)
            .SetValidator(new CreateWorkoutPhaseDtoValidator())
            .When(x => x.Phases != null);

        // Business rule: UserCreated workouts must have CreatedByUserId
        RuleFor(x => x)
            .Must(x => x.Type != WorkoutType.UserCreated || x.CreatedByUserId.HasValue)
            .WithMessage("User-created workouts must have a valid CreatedByUserId");

        // Business rule: Fixed workouts created by coach must have CreatedByCoachId
        RuleFor(x => x)
            .Must(x => x.Type != WorkoutType.Fixed || x.CreatedByCoachId.HasValue || x.CreatedByUserId.HasValue)
            .WithMessage("Fixed workouts must have either CreatedByUserId or CreatedByCoachId");
    }
}

public class CreateWorkoutPhaseDtoValidator : AbstractValidator<CreateWorkoutPhaseDto>
{
    public CreateWorkoutPhaseDtoValidator()
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

        RuleFor(x => x.Exercises)
            .Must(exercises => exercises == null || exercises.Count <= 50)
            .WithMessage("Maximum 50 exercises are allowed per phase");

        RuleForEach(x => x.Exercises)
            .SetValidator(new CreateWorkoutExerciseDtoValidator())
            .When(x => x.Exercises != null);
    }
}

public class CreateWorkoutExerciseDtoValidator : AbstractValidator<CreateWorkoutExerciseDto>
{
    public CreateWorkoutExerciseDtoValidator()
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
