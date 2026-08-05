namespace OnePieceMap.Application.Common;

// RN12: locale resolution with fallback, resolved in C# (not SQL) — same pattern as RN05.
public static class LocaleResolver
{
    public const string DefaultLocale = "en";

    public static string Resolve<TTranslation>(
        string defaultValue,
        Dictionary<string, TTranslation>? translations,
        string? locale,
        Func<TTranslation, string> select)
        => !string.IsNullOrEmpty(locale)
            && locale != DefaultLocale
            && translations is not null
            && translations.TryGetValue(locale, out var translation)
                ? select(translation)
                : defaultValue;
}
