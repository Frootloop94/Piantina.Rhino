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
            Spacing = new Eto.Drawing.Size(10, 4),

            Rows =
    {
        new TableRow(
            new TableCell(_label, true),
            new TableCell(_value, false))
    }
        };
    }
}