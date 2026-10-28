using Eto.Forms;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

public class InfoRow : Panel
{
    private readonly Label _label;
    private readonly Label _value;

    public string Label
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public string Value
    {
        get => _value.Text;
        set => _value.Text = value;
    }

    public InfoRow(string label, string value)
    {
        _label = new Label
        {
            Text = label,
            Font = AppFonts.Body,
            TextColor = AppColors.TextMuted
        };

        _value = new Label
        {
            Text = value,
            Font = AppFonts.BodyBold,
            TextColor = AppColors.Text,
            TextAlignment = TextAlignment.Right
        };

        Content = new TableLayout
        {
            Padding = 0,
            Spacing = new Eto.Drawing.Size(6, 4),

            Rows =
    {
        new TableRow(
            new TableCell(_label, false),
            new TableCell(_value, true))
    }
        };
    }

    /// <summary>
    /// Fixes both columns to an exact width instead of each one sizing to
    /// its own label/value text, so a group of InfoRows stacked in the same
    /// card (each one its own independent TableLayout) lines up into a
    /// clean two-column table - relying on the value column's own
    /// TextAlignment.Right isn't enough on its own, since that only aligns
    /// text within whatever width this one row happens to need, not a width
    /// shared with its sibling rows above/below it.
    /// </summary>
    public void SetColumnWidths(int labelWidth, int valueWidth)
    {
        _label.Width = labelWidth;
        _value.Width = valueWidth;
    }
}