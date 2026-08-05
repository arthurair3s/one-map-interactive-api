using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnePieceMap.Infrastructure.Data.Configurations;

// Shared jsonb mapping for the per-entity Translations dictionaries (i18n — backend-planning.md §9).
// EF Core can't track in-place mutations of a Dictionary<,> by reference equality alone, so a
// ValueComparer based on JSON equality is required for change detection to work correctly.
internal static class TranslationsPropertyExtensions
{
    public static void ConfigureTranslations<TTranslation>(this PropertyBuilder<Dictionary<string, TTranslation>?> builder)
    {
        builder
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, TTranslation>>(v, (JsonSerializerOptions?)null))
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, TTranslation>?>(
                (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
                v => v == null ? 0 : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
                v => v == null ? null : new Dictionary<string, TTranslation>(v)));
    }
}
