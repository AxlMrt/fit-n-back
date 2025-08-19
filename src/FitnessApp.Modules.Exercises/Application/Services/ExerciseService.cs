using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;

namespace FitnessApp.Modules.Exercises.Application.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _repo;

        public ExerciseService(IExerciseRepository repo)
        {
            _repo = repo;
        }

        public async Task<ExerciseDto?> GetByIdAsync(Guid id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e?.ToDto();
        }

        public async Task<IEnumerable<ExerciseDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            var dtos = new List<ExerciseDto>();
            foreach (var e in list)
                dtos.Add(e.ToDto());
            return dtos;
        }

        public async Task<ExerciseDto> CreateAsync(CreateExerciseDto dto)
        {
            var entity = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Type = dto.Type,
                MuscleGroups = dto.MuscleGroups.Aggregate(Domain.Enums.MuscleGroup.NONE, (acc, mg) =>
                {
                    if (Enum.TryParse<Domain.Enums.MuscleGroup>(mg, true, out var parsed))
                        return acc | parsed;
                    return acc;
                }),
                Difficulty = (Domain.Enums.DifficultyLevel)dto.Difficulty,
                Equipment = new Domain.ValueObjects.Equipment(dto.Equipment),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            return entity.ToDto();
        }

        public async Task<ExerciseDto?> UpdateAsync(Guid id, ExerciseDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            existing = UpdateEntityFromDto(existing, dto);
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return existing.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return;
            await _repo.DeleteAsync(existing);
        }

        private static Exercise UpdateEntityFromDto(Exercise entity, ExerciseDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Name))
            {
                entity.Name = dto.Name;
            }

            entity.Type = dto.Type;

            if (dto.MuscleGroups != null && dto.MuscleGroups.Any())
            {
                entity.MuscleGroups = dto.MuscleGroups.Aggregate(Domain.Enums.MuscleGroup.NONE, (acc, mg) =>
                {
                    if (Enum.TryParse<Domain.Enums.MuscleGroup>(mg, true, out var parsed))
                        return acc | parsed;
                    return acc;
                });
            }

            if (dto.ImageContentId != null)
            {
                entity.ImageContentId = dto.ImageContentId;
            }

            if (dto.VideoContentId != null)
            {
                entity.VideoContentId = dto.VideoContentId;
            }

            entity.Difficulty = (Domain.Enums.DifficultyLevel)dto.Difficulty;

            if (dto.Equipment != null && dto.Equipment.Any())
            {
                entity.Equipment = new Domain.ValueObjects.Equipment(dto.Equipment);
            }

            entity.UpdatedAt = DateTime.UtcNow;

            return entity;
        }
    }
}
