using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class MetalPricesCard : Card
{
    public MetalPricesCard(MaterialService materialService)
        : base("Metal Prices")
    {
        // Same ordering as the Materials list (alphabetical by category, then
        // name) so the two views stay consistent for the user.
        var materials = materialService.GetMaterials()
            .OrderBy(material => material.Category.ToString(), StringComparer.OrdinalIgnoreCase)
            .ThenBy(material => material.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (materials.Count == 0)
        {
            WithContent(new Label { Text = "No active materials yet." });
            return;
        }

        WithContent(materials
            .Select(material => (Control)new InfoRow(material.Name, $"R {material.PricePerGram:N2}"))
            .ToArray());
    }
}
