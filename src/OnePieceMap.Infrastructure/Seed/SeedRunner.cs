using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Infrastructure.Seed;

// Idempotent by "clear and reinsert": every run wipes the seeded tables and reloads them
// from Seed/characters.json + Seed/factions.json (global rosters) + Seed/sagas/*.json (one
// saga's content per file), so re-running never duplicates rows.
public class SeedRunner(AppDbContext context)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task RunAsync(string seedDirectory)
    {
        var characterRoster = await ReadAsync<SeedCharacterRoster>(Path.Combine(seedDirectory, "characters.json"));
        var factionRoster = await ReadAsync<SeedFactionRoster>(Path.Combine(seedDirectory, "factions.json"));

        // No sagas yet is a legitimate state (a freshly reset seed, or before the first
        // saga file is added) — MSBuild only copies files, so a sagas/ folder with zero
        // matching files never even reaches the build output. Directory.GetFiles would
        // throw DirectoryNotFoundException on that, so check existence first.
        var sagasDirectory = Path.Combine(seedDirectory, "sagas");
        var sagaFiles = new List<SeedSagaFile>();
        if (Directory.Exists(sagasDirectory))
        {
            foreach (var path in Directory.GetFiles(sagasDirectory, "*.json"))
            {
                sagaFiles.Add(await ReadAsync<SeedSagaFile>(path));
            }
        }

        // Directory.GetFiles doesn't guarantee any particular order — chronology comes
        // from each saga's own Order field, same field the database enforces as unique.
        sagaFiles = [.. sagaFiles.OrderBy(f => f.Saga.Order)];

        await ClearAsync();

        // Characters and Factions are global: loaded once, referenced by every saga file
        // below. A recurring character (Luffy) or faction (Straw Hat Pirates) lives here
        // exactly once instead of once per saga.
        var characters = characterRoster.Characters.ToDictionary(c => c.Id, c => new Character { Name = c.Name, Slug = c.Slug });
        context.Characters.AddRange(characters.Values);
        await context.SaveChangesAsync();

        var factions = factionRoster.Factions.ToDictionary(f => f.Id, f => new Faction
        {
            Name = f.Name,
            Slug = f.Slug,
            Translations = f.Translations
        });
        context.Factions.AddRange(factions.Values);
        await context.SaveChangesAsync();

        foreach (var data in sagaFiles)
        {
            await SeedSagaAsync(data, characters, factions);
        }
    }

    // Everything below is intra-file: an Arc's CharacterVersion, an Island's ArcIsland, an
    // Event's ArcIsland all reference ids local to THIS saga's file — only CharacterId and
    // FactionId reach into the global rosters loaded once in RunAsync.
    private async Task SeedSagaAsync(SeedSagaFile data, Dictionary<int, Character> characters, Dictionary<int, Faction> factions)
    {
        var saga = new Saga { Name = data.Saga.Name, Order = data.Saga.Order, Translations = data.Saga.Translations };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        var arcs = data.Arcs.ToDictionary(a => a.Id, a => new Arc
        {
            SagaId = saga.Id,
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

    private static async Task<T> ReadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Could not parse seed file '{path}'.");
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
