using Eto.Drawing;
using Eto.Forms;

namespace Piantina.Plugin.Views;

public class SettingsView : Panel
{
    public SettingsView()
    {
        Padding = 20;

        Content = new Label
        {
            Text = "Settings",
            Font = new Font(SystemFont.Bold, 20)
        };
    }
}