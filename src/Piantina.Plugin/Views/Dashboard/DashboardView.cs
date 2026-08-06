using Eto.Forms;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI.Layouts;

namespace Piantina.Plugin.Views.Dashboard;

public class DashboardView : Panel
{


    public DashboardView()
    {
        Padding = 20;


        var pricesCard = new MetalPricesCard();

        var actionsCard = new QuickActionsCard();

        var statusCard = new SystemStatusCard();

        var projectsCard = new RecentProjectsCard();


        var grid = new CardGrid()
            .WithCards(
                new MetalPricesCard(),
                new QuickActionsCard(),
                new SystemStatusCard(),
                new RecentProjectsCard());

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