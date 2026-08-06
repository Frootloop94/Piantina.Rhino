using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.Navigation;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

public class NavigationButton : Panel
{
    private readonly Label _label;
    private readonly Panel _accentBar;

    public SidebarItem Item { get; }

    public bool IsSelected { get; private set; }

    public event EventHandler? Selected;

    public NavigationButton(
        SidebarItem item,
        Action<Control> showView)
    {
        Item = item;

        Height = 42;
        Width = -1;



        _accentBar = new Panel
        {
            Width = 3,
            Height = 42,
            BackgroundColor = Colors.Transparent
        };

        _label = new Label
        {
            Text = item.Title,
            Font = AppFonts.Body,
            VerticalAlignment = VerticalAlignment.Center
        };

        Content = new TableLayout
        {
            Padding = new Padding(0),
            Spacing = new Size(0, 0),

            Rows =
    {
        new TableRow(
            new TableCell(_accentBar, false),
            new TableCell(
                new Panel
                {
                    Padding = new Padding(12, 8),
                    Content = _label
                },
                true))
    }
        };

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

        _accentBar.BackgroundColor = AppColors.Accent;

        BackgroundColor = AppColors.Selected;

        _label.Font = new Font(
            AppFonts.Body.Family,
            AppFonts.Body.Size,
            FontStyle.Bold);
    }

    public void Deselect()
    {
        IsSelected = false;

        _accentBar.BackgroundColor = Colors.Transparent;

        BackgroundColor = Colors.Transparent;

        _label.Font = AppFonts.Body;
    }
}