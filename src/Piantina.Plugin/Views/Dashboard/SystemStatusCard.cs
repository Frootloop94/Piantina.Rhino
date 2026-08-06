using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class SystemStatusCard : Card
{
    public SystemStatusCard()
        : base("System Status")
    {
        WithContent(
            new InfoRow("Rhino", "Connected"),
            new InfoRow("Plugin", "Ready"),
            new InfoRow("Database", "Offline"),
            new InfoRow("Version", "0.1.0")
        );
    }
}