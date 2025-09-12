using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FitnessApp.API.Swagger;

/// <summary>
/// OperationFilter to properly document multipart/form-data file uploads (IFormFile) for Swashbuckle.
/// Supports both direct IFormFile parameters and complex types that contain IFormFile properties.
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation == null || context == null) return;

        // Collect parameters that either are IFormFile or contain IFormFile properties
        var formParams = new List<(ParameterInfo Param, List<PropertyInfo> FileProperties, List<PropertyInfo> OtherFormProps)>();

        foreach (var p in context.MethodInfo.GetParameters())
        {
            // direct IFormFile parameter
            if (p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[]))
            {
                formParams.Add((p, new List<PropertyInfo> { }, new List<PropertyInfo>()));
                continue;
            }

            // complex type: inspect properties for IFormFile
            var props = p.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fileProps = props.Where(pi => pi.PropertyType == typeof(IFormFile) || pi.PropertyType == typeof(IFormFile[])).ToList();
            if (fileProps.Any())
            {
                // treat other simple properties as form fields
                var otherProps = props.Where(pi => !fileProps.Contains(pi)).ToList();
                formParams.Add((p, fileProps, otherProps));
            }
        }

        if (!formParams.Any())
            return;

        // Ensure request body
        operation.RequestBody ??= new OpenApiRequestBody();

        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>()
        };

        foreach (var entry in formParams)
        {
            // direct file parameter
            if (entry.FileProperties.Count == 0 && (entry.Param.ParameterType == typeof(IFormFile) || entry.Param.ParameterType == typeof(IFormFile[])))
            {
                var name = entry.Param.Name ?? "file";
                schema.Properties[name] = new OpenApiSchema { Type = "string", Format = "binary" };
                continue;
            }

            // complex type: add each file prop as binary and other props as strings
            foreach (var fp in entry.FileProperties)
            {
                var name = fp.Name;
                schema.Properties[name] = new OpenApiSchema { Type = "string", Format = "binary" };
            }

            foreach (var op in entry.OtherFormProps)
            {
                // Only include simple scalar types; map to string for simplicity
                schema.Properties[op.Name] = new OpenApiSchema { Type = "string" };
            }
        }

        // Remove application/json if present and set multipart/form-data
        operation.RequestBody.Content.Remove("application/json");
        operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType { Schema = schema };

        // Remove any operation parameters that were moved into the request body
        var movedParamNames = formParams.Select(f => f.Param.Name).Where(n => n != null).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (operation.Parameters != null && operation.Parameters.Any())
        {
            operation.Parameters = operation.Parameters.Where(p => !movedParamNames.Contains(p.Name)).ToList();
        }
    }
}
