using Eto.Forms;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

public class PiantinaSearchBox : TextBox
{
    public PiantinaSearchBox()
    {
        PlaceholderText = "Search materials...";
        Font = AppFonts.Body;
    }
}