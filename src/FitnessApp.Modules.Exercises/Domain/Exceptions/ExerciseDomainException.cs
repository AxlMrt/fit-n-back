namespace FitnessApp.Modules.Exercises.Domain.Exceptions
{
    public class ExerciseDomainException : Exception
    {
        public ExerciseDomainException(string message) : base(message)
        {
        }

        public ExerciseDomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
