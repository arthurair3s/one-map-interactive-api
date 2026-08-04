using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data.Configurations;

public class ArcIslandConfiguration : IEntityTypeConfiguration<ArcIsland>
{
    public void Configure(EntityTypeBuilder<ArcIsland> builder)
    {
        builder.HasIndex(ai => new { ai.ArcId, ai.IslandId }).IsUnique();
        builder.HasIndex(ai => new { ai.ArcId, ai.Order }).IsUnique();

        builder.HasOne(ai => ai.Arc)
            .WithMany(a => a.ArcIslands)
            .HasForeignKey(ai => ai.ArcId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ai => ai.Island)
            .WithMany(i => i.ArcIslands)
            .HasForeignKey(ai => ai.IslandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
