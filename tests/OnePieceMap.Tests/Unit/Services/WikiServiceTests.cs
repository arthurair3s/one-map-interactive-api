using OnePieceMap.Application.Features.CharacterVersions;
using OnePieceMap.Application.Features.Wiki;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Domain.Enums;

namespace OnePieceMap.Tests.Unit.Services;

// RN09: island details carry every visit the journey has already reached — filtered by
// Arc.GlobalOrder <= the active arc's GlobalOrder, not by an exact arc match. An island
// passed earlier keeps showing what happened there; only later visits stay hidden.
public class WikiServiceTests
{
    // Loguetown is visited twice on purpose: once early (GlobalOrder 3) and once late
    // (GlobalOrder 9), which is the only shape that tells "<=" apart from both "==" and
    // "everything".
    private static async Task<(WikiService Service, int[] ArcIds)> SeedAsync()
    {
        var context = TestDbContextFactory.Create();

        var saga = new Saga { Name = "East Blue", Order = 1 };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        var arcs = new[]
        {
            new Arc { SagaId = saga.Id, Name = "Romance Dawn", Order = 1, GlobalOrder = 1 },
            new Arc { SagaId = saga.Id, Name = "Syrup Village", Order = 2, GlobalOrder = 3 },
            new Arc { SagaId = saga.Id, Name = "Arlong Park", Order = 3, GlobalOrder = 6 },
            new Arc { SagaId = saga.Id, Name = "Loguetown", Order = 4, GlobalOrder = 9 },
        };
        context.Arcs.AddRange(arcs);
        await context.SaveChangesAsync();

        var island = new Island
        {
            Name = "Loguetown", Slug = "loguetown", Description = "The town of the beginning and the end.",
            ModelUrl = "", ThumbnailUrl = "/loguetown.png", Scale = 1, IsActive = true
        };
        context.Islands.Add(island);
        await context.SaveChangesAsync();

        var firstVisit = new ArcIsland { ArcId = arcs[1].Id, IslandId = island.Id, Order = 1 };
        var secondVisit = new ArcIsland { ArcId = arcs[3].Id, IslandId = island.Id, Order = 1 };
        context.ArcIslands.AddRange(firstVisit, secondVisit);
        await context.SaveChangesAsync();

        context.Events.AddRange(
            new Event
            {
                ArcIslandId = firstVisit.Id, Title = "Usopp joins the crew",
                Description = "...", Type = EventType.Lore, Order = 1
            },
            new Event
            {
                ArcIslandId = secondVisit.Id, Title = "Luffy is nearly executed",
                Description = "...", Type = EventType.Combat, Order = 1
            });
        await context.SaveChangesAsync();

        var service = new WikiService(context, new CharacterVersionService(context));

        return (service, arcs.Select(a => a.Id).ToArray());
    }

    [Fact]
    public async Task GetIslandDetailsBySlug_AtALaterArc_StillShowsTheEarlierVisit()
    {
        var (service, arcIds) = await SeedAsync();

        // Arlong Park (GlobalOrder 6) — past Syrup Village, before Loguetown.
        var result = await service.GetIslandDetailsBySlugAsync("loguetown", arcIds[2]);

        var titles = result.Events.Select(e => e.Title).ToArray();
        Assert.Equal(["Usopp joins the crew"], titles);
    }

    [Fact]
    public async Task GetIslandDetailsBySlug_AtTheLastArc_AccumulatesEveryVisitInOrder()
    {
        var (service, arcIds) = await SeedAsync();

        var result = await service.GetIslandDetailsBySlugAsync("loguetown", arcIds[3]);

        var events = result.Events.ToArray();
        Assert.Equal(["Usopp joins the crew", "Luffy is nearly executed"], events.Select(e => e.Title));
        // Each event names the visit it belongs to, so a mixed list stays readable.
        Assert.Equal([3, 9], events.Select(e => e.ArcGlobalOrder));
        Assert.Equal(["Syrup Village", "Loguetown"], events.Select(e => e.ArcName));
    }

    [Fact]
    public async Task GetIslandDetailsBySlug_BeforeTheFirstVisit_StaysEmpty()
    {
        var (service, arcIds) = await SeedAsync();

        // Romance Dawn (GlobalOrder 1) — the reader hasn't reached this island yet.
        var result = await service.GetIslandDetailsBySlugAsync("loguetown", arcIds[0]);

        Assert.Empty(result.Events);
        Assert.Empty(result.Characters);
        // The island itself is not a secret (RN09's deliberate exception) — only what happens on it.
        Assert.Equal("Loguetown", result.Name);
    }

    // Character details apply the same RN09-style "<=" accumulation as island events, but to
    // CharacterVersion — the carousel shows every version reached so far, not just the one
    // currently effective (that's RN05, exercised elsewhere for CharacterVersionService).
    private static async Task<(WikiService Service, int[] ArcIds)> SeedCharacterAsync()
    {
        var context = TestDbContextFactory.Create();

        var saga = new Saga { Name = "East Blue", Order = 1 };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        var arcs = new[]
        {
            new Arc { SagaId = saga.Id, Name = "Romance Dawn", Order = 1, GlobalOrder = 1 },
            new Arc { SagaId = saga.Id, Name = "Syrup Village", Order = 2, GlobalOrder = 3 },
            new Arc { SagaId = saga.Id, Name = "Arlong Park", Order = 3, GlobalOrder = 6 },
            new Arc { SagaId = saga.Id, Name = "Loguetown", Order = 4, GlobalOrder = 9 },
        };
        context.Arcs.AddRange(arcs);
        await context.SaveChangesAsync();

        var faction = new Faction { Name = "Straw Hat Pirates", Slug = "straw-hat-pirates" };
        context.Factions.Add(faction);
        await context.SaveChangesAsync();

        var character = new Character { Name = "Nico Robin", Slug = "nico-robin" };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Two versions on purpose, at GlobalOrder 3 and 9 (not consecutive arcs) — the only
        // shape that tells "<=" apart from both "==" and "everything".
        context.CharacterVersions.AddRange(
            new CharacterVersion
            {
                CharacterId = character.Id, ArcId = arcs[1].Id, Alias = "Miss All Sunday",
                Epithet = "Devil Child", Bounty = 79_000_000, Status = CharacterStatus.Alive,
                FactionId = faction.Id, ImageUrl = "/robin-1.png", Description = "..."
            },
            new CharacterVersion
            {
                CharacterId = character.Id, ArcId = arcs[3].Id, Alias = "Nico Robin",
                Epithet = "Devil Child", Bounty = 80_000_000, Status = CharacterStatus.Alive,
                FactionId = faction.Id, ImageUrl = "/robin-2.png", Description = "..."
            });
        await context.SaveChangesAsync();

        var service = new WikiService(context, new CharacterVersionService(context));

        return (service, arcs.Select(a => a.Id).ToArray());
    }

    [Fact]
    public async Task GetCharacterDetailsBySlug_AtALaterArc_StillShowsTheEarlierVersion()
    {
        var (service, arcIds) = await SeedCharacterAsync();

        // Arlong Park (GlobalOrder 6) — past Syrup Village, before Loguetown.
        var result = await service.GetCharacterDetailsBySlugAsync("nico-robin", arcIds[2]);

        var aliases = result.Versions.Select(v => v.Alias).ToArray();
        Assert.Equal(["Miss All Sunday"], aliases);
    }

    [Fact]
    public async Task GetCharacterDetailsBySlug_AtTheLastArc_AccumulatesEveryVersionInOrder()
    {
        var (service, arcIds) = await SeedCharacterAsync();

        var result = await service.GetCharacterDetailsBySlugAsync("nico-robin", arcIds[3]);

        var versions = result.Versions.ToArray();
        Assert.Equal(["Miss All Sunday", "Nico Robin"], versions.Select(v => v.Alias));
        Assert.Equal([3, 9], versions.Select(v => v.ArcGlobalOrder));
        Assert.Equal(["Syrup Village", "Loguetown"], versions.Select(v => v.ArcName));
    }

    [Fact]
    public async Task GetCharacterDetailsBySlug_BeforeTheFirstVersion_StaysEmpty()
    {
        var (service, arcIds) = await SeedCharacterAsync();

        // Romance Dawn (GlobalOrder 1) — the reader hasn't met this character yet.
        var result = await service.GetCharacterDetailsBySlugAsync("nico-robin", arcIds[0]);

        Assert.Empty(result.Versions);
        // The character record itself is not a secret — only which versions have been revealed.
        Assert.Equal("Nico Robin", result.Name);
        Assert.Equal("nico-robin", result.Slug);
    }
}
