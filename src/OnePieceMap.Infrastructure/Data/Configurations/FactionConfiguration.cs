using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data.Configurations;

public class FactionConfiguration : IEntityTypeConfiguration<Faction>
{
    public void Configure(EntityTypeBuilder<Faction> builder)
    {
        builder.HasIndex(f => f.Name).IsUnique();
        builder.HasIndex(f => f.Slug).IsUnique();
        builder.Property(f => f.Translations).ConfigureTranslations();
    }
}
