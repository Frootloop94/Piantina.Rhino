using System.Collections.Generic;
using Piantina.Plugin.Views;

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