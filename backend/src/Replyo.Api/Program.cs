using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Replyo.Application;
using Replyo.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());

// Health checks read connection strings directly rather than going through Infrastructure
// because they're an HTTP/pipeline concern (MapHealthChecks below) wired in this layer.
// Acceptable duplication: the Postgres string is also read inside AddInfrastructure.
var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Postgres connection string not configured.");

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Redis connection string not configured.");

builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        postgresConnectionString,
        name: "postgres",
        tags: new[] { "ready", "db" })
    .AddRedis(
        redisConnectionString,
        name: "redis",
        tags: new[] { "ready", "cache" });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Replyo API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Liveness: no checks run, just confirms the process is alive and the pipeline is responsive.
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Readiness: runs every check tagged "ready" (postgres + redis).
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse
});

app.Run();

static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var payload = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            duration = entry.Value.Duration.TotalMilliseconds,
            description = entry.Value.Description,
            error = entry.Value.Exception?.Message
        })
    };

    return context.Response.WriteAsJsonAsync(payload);
}

public partial class Program;