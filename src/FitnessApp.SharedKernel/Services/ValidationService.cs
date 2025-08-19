using FitnessApp.SharedKernel.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessApp.SharedKernel.Services;
public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ValidateAsync<T>(T model) where T : class
    {
        IEnumerable<IValidator<T>> validators = _serviceProvider.GetServices<IValidator<T>>();
        
        if (!validators.Any())
        {
            return; 
        }

        var context = new ValidationContext<T>(model);

        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context)));

        List<FluentValidation.Results.ValidationFailure> failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }
    }
}