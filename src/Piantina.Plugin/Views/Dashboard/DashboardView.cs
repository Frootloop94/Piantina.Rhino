using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI.Layouts;

namespace Piantina.Plugin.Views.Dashboard;

public class DashboardView : Panel
{
    public DashboardView(Action<string> navigate)
    {
        Padding = 20;

        var materialService = new MaterialService();

        var pricesCard = new MetalPricesCard(materialService);

        var actionsCard = new QuickActionsCard(
            onCastingCalculator: () => navigate("Calculator"),
            onMaterials: () => navigate("Materials"),
            onManufacturing: () => navigate("Manufacturing"));

        var statusCard = new SystemStatusCard();

        var projectsCard = new RecentProjectsCard();

        // Single column: this panel is meant to dock as a narrow sidebar, where
        // there isn't room for the 2-up card grid a wide floating window could fit.
        var grid = new CardGrid { PreferredColumns = 1 }
            .WithCards(pricesCard, actionsCard, statusCard, projectsCard);

        Content = new StackLayout
        {
            Spacing = 30,

            Items =
            {
                new SectionHeader("Dashboard"),
                grid
            }
        };
    }
}
