using Rhino.UI;
using Piantina.Plugin.Panels;

namespace Piantina.Plugin.Services;

public static class PanelService
{
    public static void Show()
    {
        Rhino.UI.Panels.OpenPanel(typeof(PiantinaPanel));
    }

    public static void ShowMaterialLibrary()
    {
        Rhino.UI.Panels.OpenPanel(typeof(MaterialLibraryPanel));
    }
}