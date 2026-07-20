using Rhino;
using Rhino.Commands;

namespace Piantina.Plugin;

public class PiantinaCommand : Command
{
    public static PiantinaCommand Instance { get; private set; } = null!;

    public PiantinaCommand()
    {
        Instance = this;
    }

    public override string EnglishName => "Piantina";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        RhinoApp.WriteLine("Welcome to Piantina!");

        return Result.Success;
    }
}