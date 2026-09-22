using Eto.Drawing;
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
        _materialDetails.EditRequested += material => OpenMaterialDialog(material);
        _materialDetails.DeactivateRequested += DeactivateMaterial;

        // Stacked vertically rather than side by side: this panel is meant to
        // dock as a narrow sidebar, where there isn't room for the list and
        // details to sit next to each other.
        var layout = new DynamicLayout
        {
            Spacing = new Size(0, 20)
        };

        layout.AddRow(_materialList);
        layout.AddRow(_materialDetails);

        var addButton = new PrimaryButton("+ Add Material", () => OpenMaterialDialog(null));

        Content = new StackLayout
        {
            Spacing = 20,

            Items =
        {
            new SectionHeader("Materials"),
            addButton,
            layout
        }
        };
    }

    /// <summary>
    /// Opens the editor for a new material (existing == null) or an existing one.
    /// Because Material is a reference type, editing mutates the same instance the
    /// list and details panel already hold, so refreshing after Save is enough.
    /// </summary>
    private void OpenMaterialDialog(Material? existing)
    {
        var dialog = new MaterialEditorDialog(existing);
        var result = dialog.ShowModal(this);

        if (result is null)
            return;

        if (existing is null)
        {
            _service.AddMaterial(result);
        }
        else
        {
            _service.NotifyMaterialUpdated();
        }

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
