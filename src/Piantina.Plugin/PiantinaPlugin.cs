using Rhino.PlugIns;

namespace Piantina.Plugin;

public class PiantinaPlugin : PlugIn
{
    public static PiantinaPlugin Instance { get; private set; } = null!;

    public PiantinaPlugin()
    {
        Instance = this;
    }
}