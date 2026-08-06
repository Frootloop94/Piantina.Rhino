using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class MetalPricesCard : Card
{
    public MetalPricesCard()
        : base("Metal Prices")
    {
        WithContent(
            new InfoRow("24ct Fine Gold", "R 1 532.40"),
            new InfoRow("18ct Yellow", "R 1 148.20"),
            new InfoRow("18ct White", "R 1 171.30"),
            new InfoRow("Sterling Silver", "R 18.40"),
            new InfoRow("950 Platinum", "R 648.10")
        );
    }
}