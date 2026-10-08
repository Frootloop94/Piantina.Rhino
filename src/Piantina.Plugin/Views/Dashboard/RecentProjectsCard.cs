using Eto.Forms;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class RecentProjectsCard : Card
{
    public RecentProjectsCard()
        : base("Recent Projects")
    {
        WithContent(
            new Label { Text = "• Solitaire Ring" },
            new Label { Text = "• Wedding Band" },
            new Label { Text = "• Signet Ring" }
        );
    }
}