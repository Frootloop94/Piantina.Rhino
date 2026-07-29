using Eto.Forms;

namespace Piantina.Plugin.Controls;

public class IconLabel : StackLayout
{
    public Label Label { get; }

    public IconLabel(string text)
    {
        Orientation = Orientation.Horizontal;
        Spacing = 8;

        Label = new Label
        {
            Text = text
        };

        Items.Add(Label);
    }
}