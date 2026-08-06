using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class QuickActionsCard : Card
{
    public QuickActionsCard()
        : base("Quick Actions")
    {
        WithContent(

            new PrimaryButton("Casting Calculator"),

            new PrimaryButton("Materials"),

            new PrimaryButton("Manufacturing")

        );
    }
}