using FitnessApp.Modules.Exercises.Application.DTOs.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;

namespace FitnessApp.Modules.Exercises.Application.Services;

public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IExerciseMapper _mapper;

    public EquipmentService(IEquipmentRepository equipmentRepository, IExerciseMapper mapper)
    {
        _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EquipmentResponse> GetByIdAsync(Guid id)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id);
        if (equipment == null)
            throw new KeyNotFoundException($"Equipment with ID {id} not found");
            
        return _mapper.MapToEquipment(equipment);
    }

    public async Task<IEnumerable<EquipmentResponse>> GetAllAsync()
    {
        var equipmentList = await _equipmentRepository.GetAllAsync();
        return equipmentList.Select(_mapper.MapToEquipment);
    }

    public async Task<Guid> CreateAsync(CreateEquipmentRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        var equipment = new Equipment(
            request.Name,
            request.Description
        );
        
        // if (!string.IsNullOrEmpty(request.Category))
        //     equipment.SetCategory(request.Category);
            
        await _equipmentRepository.AddAsync(equipment);
        return equipment.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateEquipmentRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        var equipment = await _equipmentRepository.GetByIdAsync(id);
        if (equipment == null)
            throw new KeyNotFoundException($"Equipment with ID {id} not found");
            
        equipment.Update(
            request.Name ?? equipment.Name,
            request.Description ?? equipment.Description
        );
        
        // if (request.Category != null)
        //     equipment.SetCategory(request.Category);
            
        await _equipmentRepository.UpdateAsync(equipment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id);
        if (equipment == null)
            throw new KeyNotFoundException($"Equipment with ID {id} not found");
            
        await _equipmentRepository.DeleteAsync(equipment.Id);
    }

    public async Task<IEnumerable<ExerciseResponse>> GetExercisesByEquipmentIdAsync(Guid equipmentId)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
        if (equipment == null)
            throw new KeyNotFoundException($"Equipment with ID {equipmentId} not found");
            
        // Assuming equipment.Exercises contains the related exercises through the navigation property
        var exercises = equipment.Exercises.Select(ee => ee.Exercise).ToList();
        return _mapper.MapToExerciseList(exercises);
    }
}