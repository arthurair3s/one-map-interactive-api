using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data.Configurations;

public class IslandConfiguration : IEntityTypeConfiguration<Island>
{
    public void Configure(EntityTypeBuilder<Island> builder)
    {
        builder.HasIndex(i => i.Name).IsUnique();
        builder.HasIndex(i => i.Slug).IsUnique();
        builder.Property(i => i.Translations).ConfigureTranslations();
    }
}
