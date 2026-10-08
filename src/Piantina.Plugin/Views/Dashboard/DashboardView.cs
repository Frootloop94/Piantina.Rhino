using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI.Layouts;

namespace Piantina.Plugin.Views.Dashboard;

public class DashboardView : Panel
{
    private readonly MetalPricesCard _pricesCard;

    public DashboardView(Action<string> navigate, MaterialService materialService)
    {
        Padding = 20;

        _pricesCard = new MetalPricesCard(materialService);

        var actionsCard = new QuickActionsCard(
            onCastingCalculator: () => navigate("Calculator"),
            onMaterials: () => navigate("Materials"));

        var statusCard = new SystemStatusCard();

        var projectsCard = new RecentProjectsCard();

        // Single column: this panel is meant to dock as a narrow sidebar, where
        // there isn't room for the 2-up card grid a wide floating window could fit.
        var grid = new CardGrid { PreferredColumns = 1 }
            .WithCards(_pricesCard, actionsCard, statusCard, projectsCard);

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

    /// <summary>
    /// Rebuilds the price list from the (shared) material service's current
    /// data. Called by PiantinaPanel whenever the Dashboard tab is selected,
    /// same as MaterialsView's own Refresh().
    /// </summary>
    public void Refresh()
    {
        _pricesCard.Refresh();
    }
}
