namespace Piantina.Core.Materials;

/// <summary>
/// One parsed (name, price) row from a daily price-list CSV export, before
/// it's matched against the material catalog.
/// </summary>
public record PriceListRow(string Name, decimal Price);

/// <summary>
/// Parses the two-column "name, price" CSV the business exports daily from
/// its master alloy price spreadsheet. Deliberately tolerant of the export
/// quirks that sheet produces rather than demanding one exact format:
///
/// - Comma- or semicolon-delimited. Excel switches the CSV field delimiter
///   to semicolon on a machine whose regional settings use comma as the
///   decimal separator, which this business's price sheet (literal "10,00%"
///   text in a couple of material names) suggests is plausible here.
/// - Quoted fields, so a comma-delimited export still works even though some
///   material names themselves contain a comma (e.g. "18ct White Gold
///   10,00%PD").
/// - "." or "," as the price's own decimal separator.
///
/// Blank lines, a leading date/title row, a header row, and any row whose
/// price doesn't parse are skipped rather than failing the whole import -
/// callers get those back as SkippedLines to report, not an exception.
/// </summary>
public static class MaterialPriceListCsv
{
    public static (IReadOnlyList<PriceListRow> Rows, IReadOnlyList<string> SkippedLines) Parse(string csvContent)
    {
        var lines = csvContent
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => line.Trim().Length > 0)
            .ToList();

        if (lines.Count == 0)
        {
            return (Array.Empty<PriceListRow>(), Array.Empty<string>());
        }

        var delimiter = lines[0].Contains(';') ? ';' : ',';

        var rows = new List<PriceListRow>();
        var skipped = new List<string>();

        foreach (var line in lines)
        {
            var fields = SplitLine(line, delimiter);

            if (fields.Count < 2)
            {
                skipped.Add(line);
                continue;
            }

            var name = fields[0].Trim();
            var priceText = fields[1].Trim();

            if (name.Length == 0 || !TryParsePrice(priceText, out var price))
            {
                skipped.Add(line);
                continue;
            }

            rows.Add(new PriceListRow(name, price));
        }

        return (rows, skipped);
    }

    /// <summary>
    /// A normal decimal-point price parses as-is. One with only a comma
    /// (e.g. "1904,60") is treated as a comma-decimal number, not a
    /// thousands-grouped one - these daily prices never run high enough to
    /// need grouping, so there's no real ambiguity to resolve.
    /// </summary>
    private static bool TryParsePrice(string text, out decimal price)
    {
        var normalised = text.Replace(" ", "");

        if (normalised.Contains(',') && !normalised.Contains('.'))
        {
            normalised = normalised.Replace(',', '.');
        }
        else
        {
            normalised = normalised.Replace(",", "");
        }

        return decimal.TryParse(
            normalised,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out price);
    }

    /// <summary>
    /// A minimal quoted-CSV splitter - handles a quoted field (so an
    /// embedded delimiter or quote character doesn't break the split) without
    /// pulling in a full CSV library for a two-column format this simple.
    /// </summary>
    private static List<string> SplitLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }

                continue;
            }

            if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == delimiter)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());

        return fields;
    }
}
