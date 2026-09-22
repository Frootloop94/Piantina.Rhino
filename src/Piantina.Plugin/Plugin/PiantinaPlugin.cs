using Rhino.PlugIns;
using Rhino.UI;
using Piantina.Plugin.Panels;
using System.Drawing;

namespace Piantina.Plugin.Plugin;

public class PiantinaPlugin : PlugIn
{
    public static PiantinaPlugin Instance { get; private set; } = null!;

    public PiantinaPlugin()
    {
        Instance = this;
    }

    protected override LoadReturnCode OnLoad(ref string errorMessage)
    {
        Rhino.UI.Panels.RegisterPanel(
    this,
    typeof(PiantinaPanel),
    "Piantina",
    SystemIcons.Application);

        return LoadReturnCode.Success;
    }
}