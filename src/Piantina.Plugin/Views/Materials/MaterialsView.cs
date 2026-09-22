using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Materials;

public class MaterialsView : Panel
{
    private readonly MaterialService _service;
    private readonly MaterialList _materialList;
    private readonly MaterialDetails _materialDetails;

    public MaterialsView()
    {
        Padding = 20;

        _service = new MaterialService();

        _materialList = new MaterialList(_service);
        _materialDetails = new MaterialDetails();

        _materialList.MaterialSelected += _materialDetails.ShowMaterial;
        _materialDetails.EditRequested += OpenEditMaterialDialog;
        _materialDetails.DeactivateRequested += DeactivateMaterial;

        // The list scrolls in its own region rather than the whole tab
        // scrolling as one long page, so Material Details stays pinned and
        // visible at the bottom instead of getting scrolled out of view.
        var scrollableList = new Scrollable
        {
            Content = _materialList,
            Border = BorderType.None
        };

        Content = new StackLayout
        {
            Spacing = 16,

            Items =
            {
                new SectionHeader("Materials"),
                new StackLayoutItem(scrollableList, true),
                _materialDetails
            }
        };
    }

    /// <summary>
    /// Refreshes the list from the current state of the service. Called by
    /// PiantinaPanel when this tab is selected, since adding a material now
    /// happens on the Settings tab and wouldn't otherwise be reflected here
    /// until this view was rebuilt from scratch.
    /// </summary>
    public void Refresh()
    {
        _materialList.Refresh();
    }

    /// <summary>
    /// Because Material is a reference type, editing mutates the same instance
    /// the list and details panel already hold, so refreshing after Save is
    /// enough - there's nothing to re-fetch.
    /// </summary>
    private void OpenEditMaterialDialog(Material existing)
    {
        var dialog = new MaterialEditorDialog(existing);
        var result = dialog.ShowModal(this);

        if (result is null)
            return;

        _service.NotifyMaterialUpdated();

        _materialList.Refresh(result.Id);
        _materialDetails.ShowMaterial(result);
    }

    private void DeactivateMaterial(Material material)
    {
        _service.SetActive(material.Id, false);

        _materialList.Refresh();
        _materialDetails.Clear();
    }
}
