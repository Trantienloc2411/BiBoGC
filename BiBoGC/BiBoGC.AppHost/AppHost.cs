using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL container with pgAdmin UI
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume("bibogc-postgres-data");

// Database for Inventory module - name must match AddNpgsqlDbContext in Program.cs
var inventoryDb = postgres.AddDatabase("InventoryDb");

// Add BiBoGC API project with database reference
builder.AddProject<Projects.BiBoGC>("bibogc")
    .WithReference(inventoryDb)    // Injects ConnectionStrings:InventoryDb
    .WaitFor(inventoryDb);         // Wait for DB to be ready

builder.Build().Run();
