using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.Views;
using System;
using Piantina.Plugin.UI;
using Piantina.Plugin.Navigation;

namespace Piantina.Plugin.Controls;


public class Sidebar : Panel
{
    private NavigationButton? _selectedButton;

    public Sidebar(Action<Control> showView)
    {
        var sidebarItems = NavigationProvider.GetItems();

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
            var button = new NavigationButton(item, showView);

            button.Selected += NavigationButton_Selected;

            layout.Items.Add(button);
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
