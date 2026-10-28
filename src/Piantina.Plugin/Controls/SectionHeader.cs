using Eto.Forms;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Controls;

public class SectionHeader : Label
{
    public SectionHeader(string text)
    {
        Text = text;
        Font = AppFonts.Title;
    }
}