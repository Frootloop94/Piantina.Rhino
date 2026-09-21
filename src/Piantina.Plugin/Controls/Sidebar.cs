using Eto.Forms;
using Piantina.Plugin.Navigation;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;


public class Sidebar : Panel
{
    private readonly List<NavigationButton> _buttons = new();
    private NavigationButton? _selectedButton;

    public Sidebar(Action<string, Control> showView)
    {
        var sidebarItems = NavigationProvider.GetItems(showView);

        var layout = new StackLayout
        {
            Width = Metrics.SidebarWidth,
            Padding = Metrics.Padding,
            Spacing = Metrics.Spacing
        };

        layout.Items.Add(
            new Label
            {
                Text = "PIANTINA",
                Font = AppFonts.Title,
                TextAlignment = TextAlignment.Center
            });

        layout.Items.Add(
            new Label
            {
                Text = "Jewellery Toolkit",
                Font = AppFonts.Heading,
                TextAlignment = TextAlignment.Center
            });

        foreach (var item in sidebarItems)
        {
            if (item.Title == "Settings")
            {
                layout.Items.Add(
                    new Panel
                    {
                        Height = 1,
                        BackgroundColor = AppColors.Hover
                    });
            }

            var button = new NavigationButton(item, showView);

            _buttons.Add(button);

            layout.Items.Add(
                new StackLayoutItem(button, HorizontalAlignment.Stretch));
        }



        Content = layout;
    }

    /// <summary>
    /// Highlights the nav button matching the given title, deselecting whichever
    /// was previously highlighted. Called from PiantinaPanel after every
    /// navigation - whether it came from a sidebar click or a Dashboard quick
    /// action - so the sidebar always reflects the view actually on screen.
    /// </summary>
    public void Select(string title)
    {
        var button = _buttons.FirstOrDefault(b => b.Item.Title == title);

        if (button is null || button == _selectedButton)
            return;

        _selectedButton?.Deselect();

        button.Select();

        _selectedButton = button;
    }
}
