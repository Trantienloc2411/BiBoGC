using System.Text;
using AuthorizationModule.Application;
using AuthorizationModule.Infrastructure;
using AuthorizationModule.Infrastructure.Data;
using BiBoGC.Middleware;
using InventoryManagement.Application;
using InventoryManagement.Infrastructure;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sale.Application;
using Sale.Infrastructure;
using Sale.Infrastructure.Data;
using Scalar.AspNetCore;


namespace BiBoGC;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Aspire service defaults
        builder.AddServiceDefaults();


        // Check if connection string is available (from AppHost in dev, or from config in prod)
        var connectionString = builder.Configuration.GetConnectionString("InventoryDb");

        if (builder.Environment.IsDevelopment() && string.IsNullOrEmpty(connectionString))
        {
            // Development with AppHost - use Aspire's AddNpgsqlDbContext
            // Connection string will be injected by AppHost
            builder.AddNpgsqlDbContext<InventoryDbContext>("InventoryDb", configureDbContextOptions: options =>
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging(true);
            });
            builder.AddNpgsqlDbContext<AuthorizationDbContext>("AuthorizationDb", configureDbContextOptions: options =>
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging(true);
            });
            builder.AddNpgsqlDbContext<SaleDbContext>("SaleDb", configureDbContextOptions: options =>
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging(true);
            });

            // Register repositories (DbContext already registered by Aspire above)
            builder.Services.AddInventoryInfrastructureWithAspire();
            builder.Services.AddSaleInfrastructureWithAspire();
        }
        else
        {
            // Production/Staging or Development without AppHost - use direct connection string
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    "Connection string 'InventoryDb' is required. " +
                    "Please provide it in 'ConnectionStrings:InventoryDb' configuration section.");

            // Register DbContext with direct connection string
            builder.Services.AddDbContextPool<InventoryDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        5,
                        TimeSpan.FromSeconds(30),
                        null);
                });
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
            });

            builder.Services.AddDbContextPool<AuthorizationDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AuthorizationDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        5,
                        TimeSpan.FromSeconds(30),
                        null);
                });
            });

            builder.Services.AddDbContextPool<SaleDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(SaleDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        5,
                        TimeSpan.FromSeconds(30),
                        null);
                });
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
            });

            // Register repositories
            builder.Services.AddInventoryInfrastructureWithAspire();
            builder.Services.AddSaleInfrastructureWithAspire();
        }

        // Register Application layer (MediatR, Validators)
        builder.Services.AddInventoryApplication();
        builder.Services.AddAuthorizationModuleApplication();
        builder.Services.AddAuthorizationModule();
        builder.Services.AddSaleApplication();

        var jwtSection = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
                        logger.LogError(context.Exception, "JWT Authentication failed");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
                        logger.LogWarning("JWT Challenge issued");
                        return Task.CompletedTask;
                    }
                };
            });


        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => { policy.RequireRole("Administrator"); })
            .AddPolicy("SellerOrAdmin", policy => { policy.RequireRole("Administrator", "Seller"); });


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
        await app.Services.InitializeAuthorizationDatabaseAsync();
        await app.Services.InitializeSaleDatabaseAsync();

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
        app.UseAuthentication();
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