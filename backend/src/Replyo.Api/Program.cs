using Microsoft.EntityFrameworkCore;
using Replyo.Application.Common.Multitenancy;
using Replyo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Postgres connection string not configured.");

builder.Services.AddDbContext<ReplyoDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.UseVector();
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddScoped<ICurrentTenant, NoTenantCurrentTenant>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();

app.Run();

internal sealed class NoTenantCurrentTenant : ICurrentTenant
{
    public Guid? TenantId => null;
}