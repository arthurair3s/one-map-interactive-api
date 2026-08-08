using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OnePieceMap.Api.Conventions;
using OnePieceMap.Api.Middleware;
using OnePieceMap.Application.Features.ArcIslands;
using OnePieceMap.Application.Features.Arcs;
using OnePieceMap.Application.Features.CharacterVersions;
using OnePieceMap.Application.Features.Characters;
using OnePieceMap.Application.Features.EventParticipants;
using OnePieceMap.Application.Features.Events;
using OnePieceMap.Application.Features.Factions;
using OnePieceMap.Application.Features.Islands;
using OnePieceMap.Application.Features.Sagas;
using OnePieceMap.Application.Features.Wiki;
using OnePieceMap.Infrastructure.Data;
using OnePieceMap.Infrastructure.Seed;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RoutePrefixConvention("api/v1"));
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<SagaService>();
builder.Services.AddScoped<ArcService>();
builder.Services.AddScoped<IslandService>();
builder.Services.AddScoped<ArcIslandService>();
builder.Services.AddScoped<CharacterService>();
builder.Services.AddScoped<FactionService>();
builder.Services.AddScoped<CharacterVersionService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<EventParticipantService>();
builder.Services.AddScoped<WikiService>();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seedFilePath = Path.Combine(AppContext.BaseDirectory, "OnePieceMap.Infrastructure", "Seed", "seed-data.json");
    await new SeedRunner(db).RunAsync(seedFilePath);
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;
