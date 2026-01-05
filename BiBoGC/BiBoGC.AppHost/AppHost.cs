using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


var postgres = builder.AddPostgres("BiBoGCPostgresDb").WithPgAdmin();
var postgresDb = postgres.AddDatabase("BiBoGCDb");

var tempPath = Path.GetTempPath();
var dbPath = Path.Combine(tempPath, "AspireDb");
Directory.CreateDirectory(dbPath);
Console.WriteLine(dbPath);

var sqlite = builder.AddSqlite("sqlite", dbPath, "BiBoGCSqlite.db");


builder.AddProject<Projects.BiBoGC>("bibogc");

builder.Build().Run();
