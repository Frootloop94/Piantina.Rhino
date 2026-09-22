using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.Controls;
using Piantina.Plugin.Views.Dashboard;
using System.Runtime.InteropServices;

namespace Piantina.Plugin.Panels;

[Guid("F4D53D5E-52D6-4E6C-95B8-1F6D6F5E78A3")]
public class PiantinaPanel : Panel
{
    private readonly Panel _contentPanel;
    private readonly Sidebar _sidebar;

    // The single place navigation happens - swaps the content and keeps the
    // sidebar highlight in sync, whether the navigation came from a sidebar
    // click or a Dashboard quick action.
    private void ShowView(string title, Control view)
    {
        _contentPanel.Content = view;
        _sidebar.Select(title);
    }


    public PiantinaPanel()
    {
        Padding = 10;

        // Small enough to dock as a narrow Rhino sidebar panel rather than
        // needing a wide floating window - the old Size(700,500)/
        // MinimumSize(900,600) made that impossible.
        MinimumSize = new Size(200, 320);

        _contentPanel = new Panel();

        _sidebar = new Sidebar(ShowView);

        ShowView("Dashboard", new DashboardView(ShowView));

        // Nav stacked above content rather than a left-hand Splitter rail:
        // a side-by-side layout doesn't fit in a narrow docked sidebar.
        Content = new StackLayout
        {
            Spacing = 10,

            Items =
            {
                _sidebar,
                new StackLayoutItem(_contentPanel, true)
            }
        };
    }
}