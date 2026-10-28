using System.Text.RegularExpressions;

namespace Piantina.Core.Materials;

/// <summary>
/// Shorthand codes for material names, used in the Casting Calculator's
/// Result and Quick Quote rows to save space in a narrow sidebar - e.g.
/// "18ct Standard Yellow Gold" -> "18Y", "18ct White Gold 10,00%PD" ->
/// "18W10". Matches the business's own trade shorthand (karat + colour
/// letter, plus the %PD figure for a white-gold alloy) for the catalog's
/// actual gold/silver/platinum names. Anything that doesn't match a known
/// pattern - a custom material the user added, or one of the Plastic/Other
/// viewing-only extras - is returned unchanged, since there's no
/// established trade shorthand for those and guessing one would likely be
/// wrong.
/// </summary>
public static class MaterialAbbreviation
{
    private static readonly Regex WhiteGoldPdPattern =
        new(@"^(\d+)ct White Gold (\d+),\d+%PD$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StandardGoldPattern =
        new(@"^(\d+)ct Standard (Yellow|White|Rose) Gold$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FineGoldPattern =
        new(@"^(\d+)ct Fine Gold$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Dictionary<string, string> FixedAbbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fine Silver"] = "FS",
        ["Sterling Silver"] = "SS",
        ["Tarnish Resistant Silver"] = "TRS",
        ["Platinum Silver"] = "PtS",
        ["Fine Platinum"] = "FPt",
        ["Platinum AU"] = "PtAU",
        ["Platinum CU"] = "PtCU"
    };

    public static string Abbreviate(string materialName)
    {
        if (FixedAbbreviations.TryGetValue(materialName, out var fixedAbbreviation))
        {
            return fixedAbbreviation;
        }

        var whiteGoldPdMatch = WhiteGoldPdPattern.Match(materialName);

        if (whiteGoldPdMatch.Success)
        {
            return $"{whiteGoldPdMatch.Groups[1].Value}W{whiteGoldPdMatch.Groups[2].Value}";
        }

        var standardGoldMatch = StandardGoldPattern.Match(materialName);

        if (standardGoldMatch.Success)
        {
            var colourLetter = char.ToUpperInvariant(standardGoldMatch.Groups[2].Value[0]);

            return $"{standardGoldMatch.Groups[1].Value}{colourLetter}";
        }

        var fineGoldMatch = FineGoldPattern.Match(materialName);

        if (fineGoldMatch.Success)
        {
            return $"{fineGoldMatch.Groups[1].Value}Y";
        }

        return materialName;
    }
}
