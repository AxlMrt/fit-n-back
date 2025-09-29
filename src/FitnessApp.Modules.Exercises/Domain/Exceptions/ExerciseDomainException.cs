using FitnessApp.SharedKernel.Exceptions;

namespace FitnessApp.Modules.Exercises.Domain.Exceptions
{
    public sealed class ExerciseDomainException : DomainException
    {
        public ExerciseDomainException(string errorCode, string message) 
            : base("Exercises", errorCode, message)
        {
        }

        public ExerciseDomainException(string errorCode, string message, Exception innerException) 
            : base("Exercises", errorCode, message, innerException)
        {
        }

        // Factory methods for common exercise domain errors
        public static ExerciseDomainException InvalidName(string name) =>
            new("INVALID_NAME", $"Exercise name '{name}' is invalid");

        public static ExerciseDomainException NameTooLong(int maxLength) =>
            new("NAME_TOO_LONG", $"Exercise name cannot exceed {maxLength} characters");

        public static ExerciseDomainException DescriptionTooLong(int maxLength) =>
            new("DESCRIPTION_TOO_LONG", $"Description cannot exceed {maxLength} characters");

        public static ExerciseDomainException InstructionsTooLong(int maxLength) =>
            new("INSTRUCTIONS_TOO_LONG", $"Instructions cannot exceed {maxLength} characters");

        public static ExerciseDomainException NameAlreadyExists(string name) =>
            new("NAME_ALREADY_EXISTS", $"An exercise with the name '{name}' already exists");

        public static ExerciseDomainException EmptyId() =>
            new("EMPTY_ID", "Exercise ID cannot be empty");

        public static ExerciseDomainException EmptyName() =>
            new("EMPTY_NAME", "Exercise name cannot be empty");

        public static ExerciseDomainException NotFound(Guid id) =>
            new("NOT_FOUND", $"Exercise with ID '{id}' was not found");

        public static ExerciseDomainException NameRequired() =>
            new("NAME_REQUIRED", "Exercise name is required");
    }
}
