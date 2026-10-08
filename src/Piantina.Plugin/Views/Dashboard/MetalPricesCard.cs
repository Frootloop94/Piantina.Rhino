using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Dashboard;

public class MetalPricesCard : Card
{
    private readonly MaterialService _materialService;
    private readonly Panel _priceHost;

    public MetalPricesCard(MaterialService materialService)
        : base("Metal Prices")
    {
        _materialService = materialService;
        _priceHost = new Panel();

        // _priceHost is added to the Card's own content area exactly once,
        // here - Refresh() below only ever replaces _priceHost.Content
        // wholesale with a brand new layout, rather than calling
        // ClearContent()/WithContent() (Card's own DynamicLayout) again
        // later. The same Clear()+re-add pattern on that DynamicLayout is
        // already known unreliable elsewhere in this codebase (see
        // CalculatorView's _dynamicFieldsHost comment) - it's what left this
        // card blank after a tab switch until something else (e.g. a
        // resize) forced a fuller re-layout.
        WithContent(_priceHost);

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
            _priceHost.Content = new Label { Text = "No active materials yet." };
            return;
        }

        var layout = new DynamicLayout
        {
            Spacing = new Eto.Drawing.Size(0, 8)
        };

        foreach (var material in materials)
        {
            layout.AddRow(new InfoRow(material.Name, $"R {material.PricePerGram:N2}"));
        }

        _priceHost.Content = layout;
    }
}
