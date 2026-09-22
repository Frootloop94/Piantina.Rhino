using Piantina.Plugin.Services;
using Rhino;
using Rhino.Commands;

namespace Piantina.Plugin.Commands;

public class PiantinaMaterialLibraryCommand : Command
{
    public override string EnglishName => "PiantinaMaterialLibrary";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        PanelService.ShowMaterialLibrary();
        return Result.Success;
    }
}
