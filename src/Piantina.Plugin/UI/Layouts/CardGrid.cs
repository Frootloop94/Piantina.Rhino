using Eto.Drawing;
using Eto.Forms;

namespace Piantina.Plugin.UI.Layouts;

public class CardGrid : Panel
{
    private readonly List<Control> _cards = new();
    public int PreferredColumns { get; set; }

    public CardGrid()
    {
        PreferredColumns = 2;

        Padding = 0;
    }

    public CardGrid WithCard(Control card)
    {
        _cards.Add(card);

        Rebuild();

        return this;
    }

    public CardGrid WithCards(params Control[] cards)
    {
        _cards.AddRange(cards);

        Rebuild();

        return this;
    }

    private void Rebuild()
    {
        var layout = new TableLayout
        {
            Padding = 0,
            Spacing = new Size(20, 20)
        };

        for (int i = 0; i < _cards.Count; i += PreferredColumns)
        {
            var row = new TableRow();

            for (int j = 0; j < PreferredColumns; j++)
            {
                if (i + j < _cards.Count)
                    // Explicit scale=true rather than relying on the implicit
                    // Control->TableCell conversion's default, so a card always
                    // fills its column's share of the available width instead
                    // of shrinking to its content's minimum.
                    row.Cells.Add(new TableCell(_cards[i + j], true));
                else
                    row.Cells.Add(null);
            }

            layout.Rows.Add(row);
        }

        Content = layout;
    }
}