using BiBoGC.Middleware;
using InventoryManagement.Application;
using InventoryManagement.Infrastructure;
using InventoryManagement.Infrastructure.Data;
using Scalar.AspNetCore;

namespace BiBoGC;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add Aspire service defaults
        builder.AddServiceDefaults();
        builder.WebHost.UseUrls("https://*:5001", "https://*5000");
        // Register DbContext with Aspire PostgreSQL (connection string injected from AppHost)
        builder.AddNpgsqlDbContext<InventoryDbContext>("InventoryDb", configureDbContextOptions: options =>
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        });

        // Register repositories (DbContext already registered by Aspire above)
        builder.Services.AddInventoryInfrastructureWithAspire();

        // Register Application layer (MediatR, Validators)
        builder.Services.AddInventoryApplication();

        // Add Controllers
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });

        // Configure OpenAPI/Swagger
        builder.Services.AddOpenApi();

        // Add CORS for development
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        // Initialize database
        await app.Services.InitializeDatabaseAsync();

        // Configure middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            // Enable OpenAPI and Scalar in development
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("BiBoGC - Inventory Management API")
                    .WithTheme(ScalarTheme.BluePlanet)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        // Use exception handling middleware
        app.UseExceptionHandling();

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthorization();

        // Map Aspire health check endpoints
        app.MapDefaultEndpoints();

        // Map API controllers
        app.MapControllers();

        // Redirect root to API documentation
        app.MapGet("/", () => Results.Redirect("/scalar/v1"));

        app.Run();
    }
}
