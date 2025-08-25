using FluentValidation;
using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Domain.Enums;

namespace FitnessApp.Modules.Exercises.Application.Validators
{
    public class CreateExerciseDtoValidator : AbstractValidator<CreateExerciseDto>
    {
        public CreateExerciseDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Exercise name is required")
                .Length(2, 100).WithMessage("Exercise name must be between 2 and 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid exercise type");

            RuleFor(x => x.MuscleGroups)
                .Must(BeValidMuscleGroups)
                .WithMessage("Invalid muscle group specified");

            RuleFor(x => x.Difficulty)
                .IsInEnum().WithMessage("Invalid difficulty level");

            RuleFor(x => x.Equipment)
                .Must(equipment => equipment == null || equipment.Count <= 10)
                .WithMessage("Cannot specify more than 10 equipment items")
                .Must(BeValidEquipment)
                .WithMessage("Equipment items must be between 1 and 50 characters");

            RuleFor(x => x.Instructions)
                .MaximumLength(2000).WithMessage("Instructions cannot exceed 2000 characters");
        }

        private static bool BeValidMuscleGroups(List<string> muscleGroups)
        {
            if (muscleGroups == null || !muscleGroups.Any()) return true;

            var validMuscleGroups = Enum.GetValues<MuscleGroup>()
                .Where(mg => mg != MuscleGroup.NONE)
                .Select(mg => mg.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return muscleGroups.All(mg => validMuscleGroups.Contains(mg));
        }

        private static bool BeValidEquipment(List<string>? equipment)
        {
            if (equipment == null || !equipment.Any()) return true;

            return equipment.All(item => 
                !string.IsNullOrWhiteSpace(item) && 
                item.Trim().Length >= 1 && 
                item.Trim().Length <= 50);
        }
    }

    public class UpdateExerciseDtoValidator : AbstractValidator<UpdateExerciseDto>
    {
        public UpdateExerciseDtoValidator()
        {
            RuleFor(x => x.Name)
                .Length(2, 100).When(x => !string.IsNullOrEmpty(x.Name))
                .WithMessage("Exercise name must be between 2 and 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.Type)
                .IsInEnum().When(x => x.Type.HasValue)
                .WithMessage("Invalid exercise type");

            RuleFor(x => x.MuscleGroups)
                .Must(BeValidMuscleGroups).When(x => x.MuscleGroups != null)
                .WithMessage("Invalid muscle group specified");

            RuleFor(x => x.Difficulty)
                .IsInEnum().When(x => x.Difficulty.HasValue)
                .WithMessage("Invalid difficulty level");

            RuleFor(x => x.Equipment)
                .Must(equipment => equipment == null || equipment.Count <= 10)
                .WithMessage("Cannot specify more than 10 equipment items")
                .Must(BeValidEquipment)
                .WithMessage("Equipment items must be between 1 and 50 characters");

            RuleFor(x => x.Instructions)
                .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Instructions))
                .WithMessage("Instructions cannot exceed 2000 characters");
        }

        private static bool BeValidMuscleGroups(List<string>? muscleGroups)
        {
            if (muscleGroups == null || !muscleGroups.Any()) return true;

            var validMuscleGroups = Enum.GetValues<MuscleGroup>()
                .Where(mg => mg != MuscleGroup.NONE)
                .Select(mg => mg.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return muscleGroups.All(mg => validMuscleGroups.Contains(mg));
        }

        private static bool BeValidEquipment(List<string>? equipment)
        {
            if (equipment == null || !equipment.Any()) return true;

            return equipment.All(item => 
                !string.IsNullOrWhiteSpace(item) && 
                item.Trim().Length >= 1 && 
                item.Trim().Length <= 50);
        }
    }

    public class ExerciseQueryDtoValidator : AbstractValidator<ExerciseQueryDto>
    {
        public ExerciseQueryDtoValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.NameFilter)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.NameFilter))
                .WithMessage("Name filter cannot exceed 100 characters");

            RuleFor(x => x.Type)
                .IsInEnum().When(x => x.Type.HasValue)
                .WithMessage("Invalid exercise type");

            RuleFor(x => x.Difficulty)
                .IsInEnum().When(x => x.Difficulty.HasValue)
                .WithMessage("Invalid difficulty level");

            RuleFor(x => x.MuscleGroups)
                .Must(BeValidMuscleGroups).When(x => x.MuscleGroups != null)
                .WithMessage("Invalid muscle group specified");

            RuleFor(x => x.SortBy)
                .Must(BeValidSortField)
                .WithMessage("Invalid sort field");
        }

        private static bool BeValidMuscleGroups(List<string>? muscleGroups)
        {
            if (muscleGroups == null || !muscleGroups.Any()) return true;

            var validMuscleGroups = Enum.GetValues<MuscleGroup>()
                .Where(mg => mg != MuscleGroup.NONE)
                .Select(mg => mg.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return muscleGroups.All(mg => validMuscleGroups.Contains(mg));
        }

        private static bool BeValidSortField(string sortBy)
        {
            var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "name", "type", "difficulty", "createdat", "updatedat", "duration", "calories"
            };

            return validSortFields.Contains(sortBy);
        }
    }
}
