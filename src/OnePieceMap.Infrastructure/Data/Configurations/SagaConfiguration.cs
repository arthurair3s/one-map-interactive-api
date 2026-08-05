using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data.Configurations;

public class SagaConfiguration : IEntityTypeConfiguration<Saga>
{
    public void Configure(EntityTypeBuilder<Saga> builder)
    {
        builder.HasIndex(s => s.Name).IsUnique();
        builder.HasIndex(s => s.Order).IsUnique();
        builder.Property(s => s.Translations).ConfigureTranslations();
    }
}
