using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Infrastructure.Data.Configurations;

public class EventParticipantConfiguration : IEntityTypeConfiguration<EventParticipant>
{
    public void Configure(EntityTypeBuilder<EventParticipant> builder)
    {
        builder.HasKey(ep => new { ep.EventId, ep.CharacterVersionId });

        builder.HasOne(ep => ep.Event)
            .WithMany(e => e.Participants)
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ep => ep.CharacterVersion)
            .WithMany(cv => cv.Participations)
            .HasForeignKey(ep => ep.CharacterVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
