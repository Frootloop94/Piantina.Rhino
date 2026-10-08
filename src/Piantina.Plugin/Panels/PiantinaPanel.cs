using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Gems;
using Piantina.Core.Materials;
using Piantina.Plugin.Navigation;
using Piantina.Plugin.Views.Dashboard;
using Piantina.Plugin.Views.Materials;
using System.Runtime.InteropServices;

namespace Piantina.Plugin.Panels;

[Guid("F4D53D5E-52D6-4E6C-95B8-1F6D6F5E78A3")]
public class PiantinaPanel : Panel
{
    private readonly TabControl _tabControl;
    private readonly Dictionary<string, int> _pageIndexByTitle = new();
    private DashboardView? _dashboardView;
    private MaterialsView? _materialsView;

    public PiantinaPanel()
    {
        Padding = 10;

        // Small enough to dock as a narrow Rhino sidebar panel rather than
        // needing a wide floating window - the old Size(700,500)/
        // MinimumSize(900,600) made that impossible.
        MinimumSize = new Size(200, 320);

        _tabControl = new TabControl();

        // One shared instance per service, not one per tab - each view used
        // to construct its own MaterialService/GemstoneService, which loads
        // its own in-memory copy of the catalog once and never re-reads it.
        // Since every tab's view is built once and kept alive for the
        // plugin's lifetime, that meant a change made on one tab (e.g. a CSV
        // price import on Settings) silently never reached another tab
        // (e.g. Dashboard's price list) until Rhino was restarted. Sharing
        // one instance means every tab is reading/writing the same in-memory
        // data, so a tab just needs telling to rebuild its own UI from it -
        // see the SelectedIndexChanged handler below.
        var materialService = new MaterialService();
        var gemstoneService = new GemstoneService();

        var index = 0;

        foreach (var item in NavigationProvider.GetItems(NavigateToSection, materialService, gemstoneService))
        {
            var view = item.CreateView();

            if (view is DashboardView dashboardView)
            {
                _dashboardView = dashboardView;
            }

            if (view is MaterialsView materialsView)
            {
                _materialsView = materialsView;
            }

            var page = new TabPage
            {
                Text = item.Title,
                // Each section scrolls independently rather than getting cut
                // off - a narrow docked panel is often shorter than a section's
                // full content. MaterialsView is the exception: it manages its
                // own internal scrolling (a scrollable list with a details
                // panel pinned below it), so it isn't wrapped again here.
                Content = view is MaterialsView
                    ? view
                    : new Scrollable { Content = view, Border = BorderType.None }
            };

            _tabControl.Pages.Add(page);
            _pageIndexByTitle[item.Title] = index;
            index++;
        }

        // Adding a material, syncing defaults, or importing prices now
        // happens on the Settings tab, but Dashboard/Materials' views were
        // already built and stay alive while their tab isn't selected - so
        // each needs telling to rebuild its own UI once the user actually
        // switches back to it, even though (now that the services are
        // shared - see above) the underlying data is already current.
        _tabControl.SelectedIndexChanged += (_, _) =>
        {
            if (_pageIndexByTitle.TryGetValue("Dashboard", out var dashboardIndex) &&
                _tabControl.SelectedIndex == dashboardIndex)
            {
                _dashboardView?.Refresh();
            }

            if (_pageIndexByTitle.TryGetValue("Materials", out var materialsIndex) &&
                _tabControl.SelectedIndex == materialsIndex)
            {
                _materialsView?.Refresh();
                _materialsView?.UpdateAvailableWidth();
            }
        };

        Content = _tabControl;

        // This panel is the control Rhino's docking manager actually resizes
        // when the user drags the sidebar wider/narrower, so its SizeChanged
        // is the one resize notification this plugin can rely on - nested
        // Scrollables further down don't always get their own. AsyncInvoke
        // defers the read until just after this resize has finished
        // propagating through the layout, so the Materials grid measures its
        // real new width rather than a stale one.
        SizeChanged += (_, _) =>
            Eto.Forms.Application.Instance.AsyncInvoke(() => _materialsView?.UpdateAvailableWidth());
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
