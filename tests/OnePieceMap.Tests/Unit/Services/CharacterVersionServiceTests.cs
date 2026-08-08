using OnePieceMap.Application.Features.CharacterVersions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Domain.Enums;

namespace OnePieceMap.Tests.Unit.Services;

// RN05: the effective version for (character, activeArc) is the one whose Arc.GlobalOrder
// is the largest value <= the active arc's GlobalOrder — i.e. resolution uses GlobalOrder,
// not the saga-local Order, and returns null when the character isn't introduced yet.
public class CharacterVersionServiceTests
{
    private static async Task<(CharacterVersionService Service, int LuffyId, int RomanceDawnArcId, int OrangeTownArcId, int SyrupVillageArcId)> SeedAsync()
    {
        var context = TestDbContextFactory.Create();

        var saga = new Saga { Name = "East Blue", Order = 1 };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        var romanceDawn = new Arc { SagaId = saga.Id, Name = "Romance Dawn", Order = 1, GlobalOrder = 1 };
        var orangeTown = new Arc { SagaId = saga.Id, Name = "Orange Town", Order = 2, GlobalOrder = 2 };
        var syrupVillage = new Arc { SagaId = saga.Id, Name = "Syrup Village", Order = 3, GlobalOrder = 3 };
        context.Arcs.AddRange(romanceDawn, orangeTown, syrupVillage);
        await context.SaveChangesAsync();

        var luffy = new Character { Name = "Monkey D. Luffy", Slug = "luffy" };
        context.Characters.Add(luffy);
        var strawHats = new Faction { Name = "Straw Hat Pirates", Slug = "straw-hat-pirates" };
        context.Factions.Add(strawHats);
        await context.SaveChangesAsync();

        // Luffy only gets a new relevant version starting at Romance Dawn (GlobalOrder 1)
        // and again at Syrup Village (GlobalOrder 3) — nothing changes at Orange Town.
        context.CharacterVersions.AddRange(
            new CharacterVersion
            {
                CharacterId = luffy.Id, ArcId = romanceDawn.Id, Alias = "Luffy", Epithet = "Straw Hat",
                Status = CharacterStatus.Alive, Faction = strawHats, ImageUrl = "/luffy-1.png", Description = "..."
            },
            new CharacterVersion
            {
                CharacterId = luffy.Id, ArcId = syrupVillage.Id, Alias = "Luffy", Epithet = "Straw Hat Luffy",
                Status = CharacterStatus.Alive, Faction = strawHats, ImageUrl = "/luffy-2.png", Description = "..."
            });
        await context.SaveChangesAsync();

        return (new CharacterVersionService(context), luffy.Id, romanceDawn.Id, orangeTown.Id, syrupVillage.Id);
    }

    [Fact]
    public async Task GetEffectiveVersionAsync_AtIntroductionArc_ReturnsThatVersion()
    {
        var (service, luffyId, romanceDawnArcId, _, _) = await SeedAsync();

        var result = await service.GetEffectiveVersionAsync(luffyId, romanceDawnArcId);

        Assert.NotNull(result);
        Assert.Equal("/luffy-1.png", result.ImageUrl);
    }

    [Fact]
    public async Task GetEffectiveVersionAsync_BetweenVersions_CarriesForwardTheLastOne()
    {
        var (service, luffyId, _, orangeTownArcId, _) = await SeedAsync();

        var result = await service.GetEffectiveVersionAsync(luffyId, orangeTownArcId);

        Assert.NotNull(result);
        Assert.Equal("/luffy-1.png", result.ImageUrl);
    }

    [Fact]
    public async Task GetEffectiveVersionAsync_AtNewVersionArc_ReturnsTheNewVersion()
    {
        var (service, luffyId, _, _, syrupVillageArcId) = await SeedAsync();

        var result = await service.GetEffectiveVersionAsync(luffyId, syrupVillageArcId);

        Assert.NotNull(result);
        Assert.Equal("/luffy-2.png", result.ImageUrl);
    }

    [Fact]
    public async Task GetEffectiveVersionAsync_CharacterNotYetIntroduced_ReturnsNull()
    {
        var context = TestDbContextFactory.Create();

        var saga = new Saga { Name = "East Blue", Order = 1 };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        var romanceDawn = new Arc { SagaId = saga.Id, Name = "Romance Dawn", Order = 1, GlobalOrder = 1 };
        var arlongPark = new Arc { SagaId = saga.Id, Name = "Arlong Park", Order = 5, GlobalOrder = 5 };
        context.Arcs.AddRange(romanceDawn, arlongPark);
        await context.SaveChangesAsync();

        var nami = new Character { Name = "Nami", Slug = "nami" };
        context.Characters.Add(nami);
        var strawHats = new Faction { Name = "Straw Hat Pirates", Slug = "straw-hat-pirates" };
        context.Factions.Add(strawHats);
        await context.SaveChangesAsync();

        // Nami's first relevant version only appears at Arlong Park (GlobalOrder 5).
        context.CharacterVersions.Add(new CharacterVersion
        {
            CharacterId = nami.Id, ArcId = arlongPark.Id, Alias = "Nami", Epithet = "Cat Burglar",
            Status = CharacterStatus.Alive, Faction = strawHats, ImageUrl = "/nami-1.png", Description = "..."
        });
        await context.SaveChangesAsync();

        var service = new CharacterVersionService(context);

        var result = await service.GetEffectiveVersionAsync(nami.Id, romanceDawn.Id);

        Assert.Null(result);
    }
}
