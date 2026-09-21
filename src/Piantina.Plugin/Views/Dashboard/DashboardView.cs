using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI.Layouts;
using Piantina.Plugin.Views.Calculator;
using Piantina.Plugin.Views.Materials;

namespace Piantina.Plugin.Views.Dashboard;

public class DashboardView : Panel
{
    public DashboardView(Action<Control> navigate)
    {
        Padding = 20;

        var materialService = new MaterialService();

        var pricesCard = new MetalPricesCard(materialService);

        var actionsCard = new QuickActionsCard(
            onCastingCalculator: () => navigate(new CalculatorView()),
            onMaterials: () => navigate(new MaterialsView()),
            onManufacturing: () => navigate(new ManufacturingView()));

        var statusCard = new SystemStatusCard();

        var projectsCard = new RecentProjectsCard();

        var grid = new CardGrid()
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
