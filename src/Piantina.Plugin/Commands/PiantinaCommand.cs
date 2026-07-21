using Piantina.Plugin.Services;
using Rhino;
using Rhino.Commands;

namespace Piantina.Plugin.Commands;

public class PiantinaCommand : Command
{
    public override string EnglishName => "Piantina";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        PanelService.Show();
        return Result.Success;
    }
}