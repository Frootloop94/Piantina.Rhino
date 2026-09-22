using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.Navigation;
using System.Runtime.InteropServices;

namespace Piantina.Plugin.Panels;

[Guid("F4D53D5E-52D6-4E6C-95B8-1F6D6F5E78A3")]
public class PiantinaPanel : Panel
{
    private readonly TabControl _tabControl;
    private readonly Dictionary<string, int> _pageIndexByTitle = new();

    public PiantinaPanel()
    {
        Padding = 10;

        // Small enough to dock as a narrow Rhino sidebar panel rather than
        // needing a wide floating window - the old Size(700,500)/
        // MinimumSize(900,600) made that impossible.
        MinimumSize = new Size(200, 320);

        _tabControl = new TabControl();

        var index = 0;

        foreach (var item in NavigationProvider.GetItems(NavigateToSection))
        {
            var page = new TabPage
            {
                Text = item.Title,
                // Each section scrolls independently rather than getting cut
                // off - a narrow docked panel is often shorter than a section's
                // full content (e.g. Materials' list plus details).
                Content = new Scrollable
                {
                    Content = item.CreateView(),
                    Border = BorderType.None
                }
            };

            _tabControl.Pages.Add(page);
            _pageIndexByTitle[item.Title] = index;
            index++;
        }

        Content = _tabControl;
    }

    /// <summary>
    /// Lets Dashboard's quick actions jump to another section the same way
    /// clicking its tab does. Each section's view was already built once when
    /// the panel opened, so this only needs to change which tab is selected.
    /// </summary>
    private void NavigateToSection(string title)
    {
        if (_pageIndexByTitle.TryGetValue(title, out var pageIndex))
        {
            _tabControl.SelectedIndex = pageIndex;
        }
    }
}
