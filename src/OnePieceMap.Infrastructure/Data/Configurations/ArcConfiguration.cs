using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data.Configurations;

public class ArcConfiguration : IEntityTypeConfiguration<Arc>
{
    public void Configure(EntityTypeBuilder<Arc> builder)
    {
        builder.HasIndex(a => new { a.SagaId, a.Order }).IsUnique();
        builder.HasIndex(a => a.GlobalOrder).IsUnique();

        builder.HasOne(a => a.Saga)
            .WithMany(s => s.Arcs)
            .HasForeignKey(a => a.SagaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
