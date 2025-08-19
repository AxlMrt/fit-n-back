using FitnessApp.API.Extensions;
using FitnessApp.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddOpenApiServices();
builder.Services.AddApiServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddJwtAuthorization(builder.Configuration);
builder.Services.AddCorsPolicy();

// Add distributed cache for token revocation
builder.Services.AddDistributedMemoryCache();

builder.Services.RegisterModules(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
app.UseModules();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApiUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowSpecificOrigins");

// Add token revocation middleware before authentication
app.UseMiddleware<TokenValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();