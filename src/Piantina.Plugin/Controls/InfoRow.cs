using Eto.Forms;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

public class InfoRow : Panel
{
    private readonly Label _label;
    private readonly Label _value;

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
            TextColor = AppColors.TextMuted
        };

        _value = new Label
        {
            Text = value,
            TextColor = AppColors.Text,
            TextAlignment = TextAlignment.Right
        };

        Content = new TableLayout
        {
            Rows =
            {
                new TableRow(
                    _label,
                    _value)
            }
        };
    }
}