using Eto.Forms;
using Piantina.Plugin.Navigation;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;


public class Sidebar : Panel
{
    private NavigationButton? _selectedButton;

    public Sidebar(Action<Control> showView)
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

            button.Selected += NavigationButton_Selected;

            layout.Items.Add(
                new StackLayoutItem(button, HorizontalAlignment.Stretch));
        }



        Content = layout;
    }
    private void NavigationButton_Selected(object? sender, EventArgs e)
    {
        if (sender is not NavigationButton button)
            return;

        _selectedButton?.Deselect();

        button.Select();

        _selectedButton = button;
    }
}
