using OnePieceMap.Application.Common;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Tests.Unit;

// RN12: locale resolution with fallback to the default (en) column.
public class LocaleResolverTests
{
    private static readonly Dictionary<string, IslandTranslation> Translations = new()
    {
        ["pt"] = new IslandTranslation("Vila Fuscia", "Uma vila pacata."),
    };

    [Fact]
    public void Resolve_WithNoLocale_FallsBackToDefault()
    {
        var result = LocaleResolver.Resolve("Fuscia Village", Translations, locale: null, t => t.Name);

        Assert.Equal("Fuscia Village", result);
    }

    [Fact]
    public void Resolve_WithLocalePresentButNoTranslation_FallsBackToDefault()
    {
        var result = LocaleResolver.Resolve<IslandTranslation>("Fuscia Village", translations: null, locale: "pt", t => t.Name);

        Assert.Equal("Fuscia Village", result);
    }

    [Fact]
    public void Resolve_WithDefaultLocaleExplicit_FallsBackToDefault()
    {
        var result = LocaleResolver.Resolve("Fuscia Village", Translations, locale: "en", t => t.Name);

        Assert.Equal("Fuscia Village", result);
    }

    [Fact]
    public void Resolve_WithTranslatedLocale_UsesTranslatedValue()
    {
        var result = LocaleResolver.Resolve("Fuscia Village", Translations, locale: "pt", t => t.Name);

        Assert.Equal("Vila Fuscia", result);
    }

    [Fact]
    public void Resolve_WithLocaleMissingFromTranslations_FallsBackToDefault()
    {
        var result = LocaleResolver.Resolve("Fuscia Village", Translations, locale: "es", t => t.Name);

        Assert.Equal("Fuscia Village", result);
    }
}
