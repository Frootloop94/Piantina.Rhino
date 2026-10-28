using Piantina.Core.Gems;
using Piantina.Core.Materials;
using Piantina.Plugin.Views;
using Piantina.Plugin.Views.Calculator;
using Piantina.Plugin.Views.Dashboard;
using Piantina.Plugin.Views.Materials;


namespace Piantina.Plugin.Navigation;

public static class NavigationProvider
{
    /// <summary>
    /// materialService/gemstoneService are shared across every section rather
    /// than each view constructing its own - they're built once in
    /// PiantinaPanel and threaded through here, so an edit made on one tab
    /// (e.g. a CSV price import on Settings) is immediately visible to every
    /// other tab's data once it's told to rebuild, instead of each tab
    /// quietly holding its own stale in-memory snapshot from whenever its
    /// view was first created.
    /// </summary>
    public static List<NavigationItem> GetItems(
        Action<string> navigate,
        MaterialService materialService,
        GemstoneService gemstoneService)
    {
        return new List<NavigationItem>
        {
            new("Dashboard", () => new DashboardView(navigate, materialService)),
            new("Materials", () => new MaterialsView(materialService, gemstoneService)),
            new("Calculator", () => new CalculatorView(materialService)),
            new("Settings", () => new SettingsView(materialService, gemstoneService))
        };
    }
}
