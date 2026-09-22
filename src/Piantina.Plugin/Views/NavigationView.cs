using Eto.Drawing;
using Eto.Forms;

namespace Piantina.Plugin.Views;

public class NavigationView : Panel
{
    public NavigationView()
    {
        Padding = 20;

        Content = new Label
        {
            Text = "Navigation",
            Font = new Font(SystemFont.Bold, 20)
        };
    }
}