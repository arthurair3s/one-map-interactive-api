using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasIndex(e => new { e.ArcIslandId, e.Order }).IsUnique();
        builder.Property(e => e.Translations).ConfigureTranslations();

        builder.HasOne(e => e.ArcIsland)
            .WithMany(ai => ai.Events)
            .HasForeignKey(e => e.ArcIslandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
