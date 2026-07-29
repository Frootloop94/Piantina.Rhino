using Eto.Forms;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views;

public class DashboardView : Panel
{


    public DashboardView()
    {
        Padding = 20;

        var welcomeCard = new Card("Welcome")
        .WithContent(
        new Label
        {
            Text = "Welcome to Piantina."
        },
        new Label
        {
            Text = "Select a section from the navigation menu."
        });

        Content = new StackLayout
        {
            Spacing = 20,

            Items =
{
    new SectionHeader("Dashboard"),
    welcomeCard
}
        };
    }
}