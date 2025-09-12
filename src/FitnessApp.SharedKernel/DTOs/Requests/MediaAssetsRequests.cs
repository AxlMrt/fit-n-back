using Microsoft.AspNetCore.Http;

namespace FitnessApp.SharedKernel.DTOs.Requests;

public sealed record MediaAssetUploadRequest(
    IFormFile File,
    Guid ExerciseId,
    string? Description
);
