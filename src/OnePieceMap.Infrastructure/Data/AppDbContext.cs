using Microsoft.EntityFrameworkCore;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Saga> Sagas => Set<Saga>();
    public DbSet<Arc> Arcs => Set<Arc>();
    public DbSet<Island> Islands => Set<Island>();
    public DbSet<ArcIsland> ArcIslands => Set<ArcIsland>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Faction> Factions => Set<Faction>();
    public DbSet<CharacterVersion> CharacterVersions => Set<CharacterVersion>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
