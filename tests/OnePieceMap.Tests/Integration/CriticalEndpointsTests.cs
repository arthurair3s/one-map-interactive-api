using System.Net;
using System.Net.Http.Json;
using OnePieceMap.Application.Features.Events;
using OnePieceMap.Application.Features.Wiki;

namespace OnePieceMap.Tests.Integration;

[Collection("Api")]
public class CriticalEndpointsTests(ApiFactoryFixture fixture)
{
    // RN11: /wiki/map returns every seeded island already revealed, each carrying the
    // correct firstAppearanceGlobalOrder, ordered ascending by it.
    [Fact]
    public async Task GetWikiMap_ReturnsAllIslandsWithFirstAppearanceGlobalOrder()
    {
        var client = fixture.CreateClient();

        var islands = await client.GetFromJsonAsync<List<WikiMapItemDto>>("/api/v1/wiki/map");

        Assert.NotNull(islands);
        Assert.Equal(6, islands.Count);
        Assert.Equal(islands.OrderBy(i => i.FirstAppearanceGlobalOrder).Select(i => i.Id), islands.Select(i => i.Id));

        var fuscia = islands.Single(i => i.Name == "Fuscia Village");
        Assert.Equal(1, fuscia.FirstAppearanceGlobalOrder);
    }

    // RN08: GET /islands/{id}/details without ?arcId= must fail with 400, not fall through
    // to the service layer.
    [Fact]
    public async Task GetIslandDetails_WithoutArcId_ReturnsBadRequest()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/islands/1/details");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // RN07: creating an Event against a non-existent ArcIslandId must fail, not silently
    // create an orphaned row.
    [Fact]
    public async Task CreateEvent_WithNonExistentArcIslandId_ReturnsNotFound()
    {
        var client = fixture.CreateClient();
        var dto = new CreateEventDto(ArcIslandId: 999_999, Title: "Ghost event", Description: "...", Type: "Lore", Order: 1);

        var response = await client.PostAsJsonAsync("/api/v1/events", dto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
