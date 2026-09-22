using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Materials;

public class MaterialsView : Panel
{
    private readonly MaterialService _service;
    private readonly MaterialList _materialList;
    private readonly MaterialDetails _materialDetails;
    private readonly Label _defaultsStatusLabel;

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

        var syncDefaultsButton = new Button { Text = "Sync Default Metals" };
        syncDefaultsButton.Click += (_, _) => SyncDefaults();

        _defaultsStatusLabel = new Label
        {
            Font = AppFonts.Small,
            TextColor = AppColors.TextMuted
        };

        var buttonRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Items = { addButton, syncDefaultsButton }
        };

        Content = new StackLayout
        {
            Spacing = 20,

            Items =
        {
            new SectionHeader("Materials"),
            buttonRow,
            _defaultsStatusLabel,
            layout
        }
        };
    }

    /// <summary>
    /// Brings the catalog in line with the current built-in default list:
    /// adds any of the business's real metals that aren't already present by
    /// name, deactivates any still-active material left over from an older
    /// version of the default list (e.g. the old generic "18ct Yellow"
    /// placeholder, superseded by "18ct Standard Yellow Gold"), and repairs
    /// the appearance of any material still stuck at the plain grey class
    /// default from before appearance colours existed (e.g. "24ct Fine Gold"
    /// showing up white/grey instead of gold). Doesn't touch anything the
    /// user added or edited themselves.
    /// </summary>
    private void SyncDefaults()
    {
        var addedCount = _service.AddMissingDefaults();
        var deactivatedCount = _service.DeactivateLegacyDefaults();
        var repairedCount = _service.RepairMissingAppearance();

        var messages = new List<string>();

        if (addedCount > 0)
        {
            messages.Add(addedCount == 1 ? "Added 1 default metal." : $"Added {addedCount} default metals.");
        }

        if (deactivatedCount > 0)
        {
            messages.Add(deactivatedCount == 1
                ? "Deactivated 1 old default."
                : $"Deactivated {deactivatedCount} old defaults.");
        }

        if (repairedCount > 0)
        {
            messages.Add(repairedCount == 1
                ? "Fixed 1 material's colour."
                : $"Fixed {repairedCount} materials' colours.");
        }

        _defaultsStatusLabel.Text = messages.Count == 0
            ? "Default metals are already in sync."
            : string.Join(" ", messages);

        _materialList.Refresh();
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
