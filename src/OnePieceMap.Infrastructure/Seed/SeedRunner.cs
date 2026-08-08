using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Infrastructure.Seed;

// Idempotent by "clear and reinsert": every run wipes the seeded tables and
// reloads them from seed-data.json, so re-running never duplicates rows.
public class SeedRunner(AppDbContext context)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task RunAsync(string seedFilePath)
    {
        var json = await File.ReadAllTextAsync(seedFilePath);
        var data = JsonSerializer.Deserialize<SeedData>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Could not parse seed file '{seedFilePath}'.");

        await ClearAsync();

        var sagas = data.Sagas.ToDictionary(s => s.Id, s => new Saga { Name = s.Name, Order = s.Order, Translations = s.Translations });
        context.Sagas.AddRange(sagas.Values);
        await context.SaveChangesAsync();

        var arcs = data.Arcs.ToDictionary(a => a.Id, a => new Arc
        {
            SagaId = sagas[a.SagaId].Id,
            Name = a.Name,
            Order = a.Order,
            GlobalOrder = a.GlobalOrder,
            Translations = a.Translations
        });
        context.Arcs.AddRange(arcs.Values);
        await context.SaveChangesAsync();

        var islands = data.Islands.ToDictionary(i => i.Id, i => new Island
        {
            Name = i.Name,
            Slug = i.Slug,
            Description = i.Description,
            CoordinateX = i.CoordinateX,
            CoordinateY = i.CoordinateY,
            CoordinateZ = i.CoordinateZ,
            RotationY = i.RotationY,
            Scale = i.Scale,
            ModelUrl = i.ModelUrl,
            ThumbnailUrl = i.ThumbnailUrl,
            IsActive = i.IsActive,
            Translations = i.Translations
        });
        context.Islands.AddRange(islands.Values);
        await context.SaveChangesAsync();

        var arcIslands = data.ArcIslands.ToDictionary(ai => ai.Id, ai => new ArcIsland
        {
            ArcId = arcs[ai.ArcId].Id,
            IslandId = islands[ai.IslandId].Id,
            Order = ai.Order
        });
        context.ArcIslands.AddRange(arcIslands.Values);
        await context.SaveChangesAsync();

        var characters = data.Characters.ToDictionary(c => c.Id, c => new Character { Name = c.Name, Slug = c.Slug });
        context.Characters.AddRange(characters.Values);
        await context.SaveChangesAsync();

        var factions = data.Factions.ToDictionary(f => f.Id, f => new Faction
        {
            Name = f.Name,
            Slug = f.Slug,
            Translations = f.Translations
        });
        context.Factions.AddRange(factions.Values);
        await context.SaveChangesAsync();

        var characterVersions = data.CharacterVersions.ToDictionary(cv => cv.Id, cv => new CharacterVersion
        {
            CharacterId = characters[cv.CharacterId].Id,
            ArcId = arcs[cv.ArcId].Id,
            Alias = cv.Alias,
            Epithet = cv.Epithet,
            Bounty = cv.Bounty,
            Status = cv.Status,
            FactionId = factions[cv.FactionId].Id,
            ImageUrl = cv.ImageUrl,
            Description = cv.Description,
            Translations = cv.Translations
        });
        context.CharacterVersions.AddRange(characterVersions.Values);
        await context.SaveChangesAsync();

        var events = data.Events.ToDictionary(e => e.Id, e => new Event
        {
            ArcIslandId = arcIslands[e.ArcIslandId].Id,
            Title = e.Title,
            Description = e.Description,
            Type = e.Type,
            Order = e.Order,
            Translations = e.Translations
        });
        context.Events.AddRange(events.Values);
        await context.SaveChangesAsync();

        var eventParticipants = data.EventParticipants.Select(ep => new EventParticipant
        {
            EventId = events[ep.EventId].Id,
            CharacterVersionId = characterVersions[ep.CharacterVersionId].Id
        });
        context.EventParticipants.AddRange(eventParticipants);
        await context.SaveChangesAsync();
    }

    // TRUNCATE ... RESTART IDENTITY resets every table's id sequence back to 1, so re-seeding
    // (e.g. every dev boot) always produces the same ids instead of drifting upward over time.
    private async Task ClearAsync()
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE "EventParticipants", "Events", "CharacterVersions", "Characters", "Factions",
                "ArcIslands", "Islands", "Arcs", "Sagas"
            RESTART IDENTITY CASCADE
            """);
    }
}
