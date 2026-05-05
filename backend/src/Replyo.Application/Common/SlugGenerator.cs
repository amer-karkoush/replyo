using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Replyo.Application.Common;

/// <summary>
/// Generates URL-safe slugs from human-readable strings.
/// </summary>
public static class SlugGenerator
{
    private static readonly Regex NonAlphanumeric = new(@"[^a-z0-9]+", RegexOptions.Compiled);
    private static readonly Regex MultipleHyphens = new(@"-+", RegexOptions.Compiled);

    /// <summary>
    /// Produces a URL-safe slug: lowercase, ASCII, hyphen-separated, no leading/trailing hyphens.
    /// Diacritics are stripped (café → cafe). Returns empty string if the input has no slug-able characters.
    /// </summary>
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Strip diacritics: "café" → "cafe"
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var ascii = sb.ToString().ToLowerInvariant();
        var hyphenated = NonAlphanumeric.Replace(ascii, "-");
        var collapsed = MultipleHyphens.Replace(hyphenated, "-");
        return collapsed.Trim('-');
    }
}