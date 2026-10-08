using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class QuickActionsCard : Card
{
    public QuickActionsCard(
        Action onCastingCalculator,
        Action onMaterials)
        : base("Quick Actions")
    {
        WithContent(

            new PrimaryButton("Casting Calculator", onCastingCalculator),

            new PrimaryButton("Materials", onMaterials)

        );
    }
}
