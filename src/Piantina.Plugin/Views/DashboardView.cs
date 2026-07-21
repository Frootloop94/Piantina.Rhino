using Eto.Drawing;
using Eto.Forms;

namespace Piantina.Plugin.Views;

public class DashboardView : Panel
{
    public DashboardView()
    {
        Padding = 20;

        Content = new StackLayout
        {
            Spacing = 12,

            Items =
            {
                new Label
                {
                    Text = "Dashboard",
                    Font = new Font(SystemFont.Bold, 20)
                },

                new Label
                {
                    Text = "Welcome to Piantina."
                },

                new Label
                {
                    Text = "Select a section from the navigation menu."
                }
            }
        };
    }
}