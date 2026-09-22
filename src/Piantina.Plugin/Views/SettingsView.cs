using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;
using Piantina.Plugin.Views.Materials;

namespace Piantina.Plugin.Views;

public class SettingsView : Panel
{
    private readonly MaterialService _service;
    private readonly Label _statusLabel;

    public SettingsView()
    {
        Padding = 20;

        _service = new MaterialService();

        var addButton = new PrimaryButton("+ Add Material", OpenAddMaterialDialog);

        var syncDefaultsButton = new Button { Text = "Sync Default Metals" };
        syncDefaultsButton.Click += (_, _) => SyncDefaults();

        _statusLabel = new Label
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
            Spacing = 12,

            Items =
            {
                new SectionHeader("Settings"),
                new Label { Text = "Materials Catalog", Font = AppFonts.Heading, TextColor = AppColors.Text },
                buttonRow,
                _statusLabel
            }
        };
    }

    /// <summary>
    /// Adding and syncing live here rather than on the Materials tab itself,
    /// to keep that tab's own top free for the material grid. The Materials
    /// tab's view was already built when the panel opened and stays alive
    /// while its tab isn't selected, so it won't see a material added here
    /// until it's told to - PiantinaPanel refreshes it when its tab is
    /// selected next.
    /// </summary>
    private void OpenAddMaterialDialog()
    {
        var dialog = new MaterialEditorDialog();
        var result = dialog.ShowModal(this);

        if (result is null)
            return;

        _service.AddMaterial(result);

        _statusLabel.Text = $"Added \"{result.Name}\".";
    }

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

        _statusLabel.Text = messages.Count == 0
            ? "Default metals are already in sync."
            : string.Join(" ", messages);
    }
}
