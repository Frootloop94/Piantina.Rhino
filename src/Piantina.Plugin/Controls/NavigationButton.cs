using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.Navigation;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

public class NavigationButton : Panel
{
    private readonly Panel _indicator;
    private readonly Label _label;

    public SidebarItem Item { get; }

    public bool IsSelected { get; private set; }

    public event EventHandler? Selected;

    public NavigationButton(
        SidebarItem item,
        Action<Control> showView)
    {
        Item = item;

        Height = 42;

        _indicator = new Panel
        {
            Width = 4
        };

        _label = new Label
        {
            Text = item.Title,
            Font = AppFonts.Body,
            VerticalAlignment = VerticalAlignment.Center
        };

        var layout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            VerticalContentAlignment = VerticalAlignment.Center,
            Spacing = 12,
            Padding = new Padding(12, 8),
            Items =
            {
                _indicator,
                _label
            }
        };

        Content = layout;

        MouseDown += (_, _) =>
        {
            showView(item.CreateView());
            Selected?.Invoke(this, EventArgs.Empty);
        };

        Deselect();
    }

    public void Select()
    {
        IsSelected = true;

        _indicator.BackgroundColor = Colors.Gold;

        _label.Font = new Font(
            AppFonts.Body.Family,
            AppFonts.Body.Size,
            FontStyle.Bold);
    }

    public void Deselect()
    {
        IsSelected = false;

        _indicator.BackgroundColor = Colors.Transparent;

        _label.Font = AppFonts.Body;
    }
}