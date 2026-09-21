using System.Reflection;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class SystemStatusCard : Card
{
    public SystemStatusCard()
        : base("System Status")
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var versionText = version is null ? "-" : $"{version.Major}.{version.Minor}.{version.Build}";

        WithContent(
            new InfoRow("Rhino", "Connected"),
            new InfoRow("Plugin", "Ready"),
            new InfoRow("Database", "Offline"),
            new InfoRow("Version", versionText)
        );
    }
}
