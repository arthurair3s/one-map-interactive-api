using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Application.Features.Sagas;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Tests.Unit.Services;

// RN01: a Saga can't be deleted while it still has Arcs linked to it.
public class SagaServiceTests
{
    [Fact]
    public async Task DeleteAsync_SagaWithArcs_ThrowsConflict()
    {
        var context = TestDbContextFactory.Create();

        var saga = new Saga { Name = "East Blue", Order = 1 };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        context.Arcs.Add(new Arc { SagaId = saga.Id, Name = "Romance Dawn", Order = 1, GlobalOrder = 1 });
        await context.SaveChangesAsync();

        var service = new SagaService(context);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(saga.Id));
    }

    [Fact]
    public async Task DeleteAsync_SagaWithoutArcs_Succeeds()
    {
        var context = TestDbContextFactory.Create();

        var saga = new Saga { Name = "East Blue", Order = 1 };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        var service = new SagaService(context);
        await service.DeleteAsync(saga.Id);

        Assert.Empty(context.Sagas);
    }

    [Fact]
    public async Task DeleteAsync_SagaDoesNotExist_ThrowsNotFound()
    {
        var context = TestDbContextFactory.Create();
        var service = new SagaService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(999));
    }
}
