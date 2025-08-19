namespace FitnessApp.SharedKernel.Interfaces;
public interface IValidationService
{
    Task ValidateAsync<T>(T model) where T : class;
}