using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

public class Card : Panel
{
    private readonly Label _headerLabel;
    private readonly Panel _headerDivider;
    private readonly DynamicLayout _contentArea;

    public Card WithContent(params Control[] controls)
    {
        foreach (var control in controls)
        {
            _contentArea.AddRow(control);
        }

        return this;
    }

    public Card ClearContent()
    {
        _contentArea.Clear();

        return this;
    }

    public Card(string title, int padding = 24, int contentSpacing = 8, int headerSpacing = 30)
    {
        Padding = new Padding(padding);
        BackgroundColor = AppColors.Card;

        _headerLabel = new Label
        {
            Text = title,
            Font = AppFonts.Heading,
            TextColor = AppColors.Text
        };

        _headerDivider = new Panel
        {
            Height = 1,
            BackgroundColor = AppColors.Hover
        };


        _contentArea = new DynamicLayout
        {
            Spacing = new Size(0, contentSpacing)
        };

        Content = new StackLayout
        {
            Spacing = headerSpacing,
            Items =
            {
                _headerLabel,
                _headerDivider,
                _contentArea
            }
        };
    }
}