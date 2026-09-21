using Piantina.Plugin.Views;
using Piantina.Plugin.Views.Calculator;
using Piantina.Plugin.Views.Dashboard;
using Piantina.Plugin.Views.Materials;


namespace Piantina.Plugin.Navigation;

public static class NavigationProvider
{
    public static List<SidebarItem> GetItems()
    {
        return new List<SidebarItem>
        {
            new("Dashboard", () => new DashboardView()),
            new("Materials", () => new MaterialsView()),
            new("Gemstones", () => new GemstonesView()),
            new("Manufacturing", () => new ManufacturingView()),
            new("Calculator", () => new CalculatorView()),
            new("Settings", () => new SettingsView())
        };
    }
}
