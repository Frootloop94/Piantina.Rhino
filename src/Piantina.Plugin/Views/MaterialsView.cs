using Eto.Drawing;
using Eto.Forms;

namespace Piantina.Plugin.Views;

public class MaterialsView : Panel
{
    public MaterialsView()
    {
        Padding = 20;

        Content = new StackLayout
        {
            Spacing = 10,

            Items =
            {
                new Label
                {
                    Text = "Materials",
                    Font = new Font(SystemFont.Bold, 20)
                },

                new Label
                {
                    Text = "This is where the metal and material library will be."
                }
            }
        };
    }
}