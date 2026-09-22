using Piantina.Plugin.Views;
using Piantina.Plugin.Views.Calculator;
using Piantina.Plugin.Views.Dashboard;
using Piantina.Plugin.Views.Materials;


namespace Piantina.Plugin.Navigation;

public static class NavigationProvider
{
    public static List<NavigationItem> GetItems(Action<string> navigate)
    {
        return new List<NavigationItem>
        {
            new("Dashboard", () => new DashboardView(navigate)),
            new("Materials", () => new MaterialsView()),
            new("Calculator", () => new CalculatorView()),
            new("Settings", () => new SettingsView())
        };
    }
}
