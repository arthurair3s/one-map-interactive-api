using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data.Configurations;

public class CharacterVersionConfiguration : IEntityTypeConfiguration<CharacterVersion>
{
    public void Configure(EntityTypeBuilder<CharacterVersion> builder)
    {
        builder.HasIndex(cv => new { cv.CharacterId, cv.ArcId }).IsUnique();

        builder.HasOne(cv => cv.Character)
            .WithMany(c => c.Versions)
            .HasForeignKey(cv => cv.CharacterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cv => cv.Arc)
            .WithMany(a => a.CharacterVersions)
            .HasForeignKey(cv => cv.ArcId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
