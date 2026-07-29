using Eto.Drawing;
using Eto.Forms;

namespace Piantina.Plugin.Views;

public class CalculatorView : Panel
{
    public CalculatorView()
    {
        Padding = 20;

        Content = new Label
        {
            Text = "Calculator",
            Font = new Font(SystemFont.Bold, 20)
        };
    }
}