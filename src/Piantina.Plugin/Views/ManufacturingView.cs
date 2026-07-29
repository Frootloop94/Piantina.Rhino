using Eto.Drawing;
using Eto.Forms;

namespace Piantina.Plugin.Views;

public class ManufacturingView : Panel
{
    public ManufacturingView()
    {
        Padding = 20;

        Content = new Label
        {
            Text = "Manufacturing",
            Font = new Font(SystemFont.Bold, 20)
        };
    }
}