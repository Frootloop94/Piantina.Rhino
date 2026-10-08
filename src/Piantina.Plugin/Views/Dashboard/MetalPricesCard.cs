using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class MetalPricesCard : Card
{
    private readonly MaterialService _materialService;

    public MetalPricesCard(MaterialService materialService)
        : base("Metal Prices")
    {
        _materialService = materialService;

        BuildContent();
    }

    /// <summary>
    /// Rebuilds the price list from the service's current data. This card is
    /// built once when the Dashboard tab's view is first created and then
    /// kept alive, so without this, prices changed elsewhere (a manual edit,
    /// Sync Default Metals, or a CSV import on the Settings tab) would never
    /// show up here until the plugin was reloaded. Called by DashboardView's
    /// own Refresh(), which PiantinaPanel calls whenever the Dashboard tab is
    /// selected.
    /// </summary>
    public void Refresh()
    {
        ClearContent();

        BuildContent();
    }

    private void BuildContent()
    {
        // Same ordering as the Materials list (category declaration order,
        // then each material's own SortOrder, then name) so the two views
        // stay consistent for the user.
        var materials = _materialService.GetMaterials()
            .OrderBy(material => (int)material.Category)
            .ThenBy(material => material.SortOrder)
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
