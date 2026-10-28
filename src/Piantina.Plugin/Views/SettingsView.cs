using Eto.Forms;
using Piantina.Core.Gems;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;
using Piantina.Plugin.Views.Gems;
using Piantina.Plugin.Views.Materials;
using System.IO;

namespace Piantina.Plugin.Views;

public class SettingsView : Panel
{
    private readonly MaterialService _materialService;
    private readonly Label _materialStatusLabel;

    private readonly GemstoneService _gemstoneService;
    private readonly Label _gemStatusLabel;

    public SettingsView(MaterialService materialService, GemstoneService gemstoneService)
    {
        Padding = 20;

        _materialService = materialService;

        var addMaterialButton = new PrimaryButton("+ Add Material", OpenAddMaterialDialog);

        var syncMetalsButton = new Button { Text = "Sync Default Metals" };
        syncMetalsButton.Click += (_, _) => SyncDefaultMetals();

        var importPricesButton = new Button { Text = "Import Prices (CSV)" };
        importPricesButton.Click += (_, _) => ImportPriceList();

        _materialStatusLabel = new Label
        {
            Font = AppFonts.Small,
            TextColor = AppColors.TextMuted
        };

        // Vertical, not side by side - three buttons together need more
        // width than a narrow docked sidebar has (see the Materials tab's
        // own button rows for the same fix).
        var materialButtonRow = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Items = { addMaterialButton, syncMetalsButton, importPricesButton }
        };

        _gemstoneService = gemstoneService;

        var addGemButton = new PrimaryButton("+ Add Gem", OpenAddGemDialog);

        var syncGemsButton = new Button { Text = "Sync Default Gems" };
        syncGemsButton.Click += (_, _) => SyncDefaultGems();

        _gemStatusLabel = new Label
        {
            Font = AppFonts.Small,
            TextColor = AppColors.TextMuted
        };

        var gemButtonRow = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Items = { addGemButton, syncGemsButton }
        };

        Content = new StackLayout
        {
            Spacing = 12,

            Items =
            {
                new SectionHeader("Settings"),
                new Label { Text = "Materials Catalog", Font = AppFonts.Heading, TextColor = AppColors.Text },
                materialButtonRow,
                _materialStatusLabel,
                new Label { Text = "Gemstones Catalog", Font = AppFonts.Heading, TextColor = AppColors.Text },
                gemButtonRow,
                _gemStatusLabel
            }
        };
    }

    /// <summary>
    /// Adding and syncing live here rather than on the Materials tab itself,
    /// to keep that tab's own top free for the material/gem grids. The
    /// Materials tab's view was already built when the panel opened and stays
    /// alive while its tab isn't selected, so it won't see something added
    /// here until it's told to - PiantinaPanel refreshes it when its tab is
    /// selected next.
    /// </summary>
    private void OpenAddMaterialDialog()
    {
        var dialog = new MaterialEditorDialog();
        var result = dialog.ShowModal(this);

        if (result is null)
            return;

        _materialService.AddMaterial(result);

        _materialStatusLabel.Text = $"Added \"{result.Name}\".";
    }

    private void SyncDefaultMetals()
    {
        var addedCount = _materialService.AddMissingDefaults();
        var deactivatedCount = _materialService.DeactivateLegacyDefaults();
        var repairedCount = _materialService.RepairMissingAppearance();

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

        _materialStatusLabel.Text = messages.Count == 0
            ? "Default metals are already in sync."
            : string.Join(" ", messages);
    }

    /// <summary>
    /// Imports today's metal price list from a CSV export of the business's
    /// master alloy price spreadsheet (see MaterialPriceListCsv for the
    /// format it tolerates). Updates PricePerGram on every material whose
    /// name matches a row; a row that doesn't match anything existing (e.g.
    /// a new alloy the catalog doesn't have yet) becomes a new material
    /// instead, rather than being silently dropped.
    /// </summary>
    private void ImportPriceList()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import Metal Price List",
            Filters = { new FileFilter("CSV files", ".csv") },
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != DialogResult.Ok)
            return;

        string csvContent;

        try
        {
            csvContent = File.ReadAllText(dialog.FileName);
        }
        catch (IOException ex)
        {
            _materialStatusLabel.Text = $"Couldn't read that file: {ex.Message}";
            return;
        }

        var (rows, skippedLines) = MaterialPriceListCsv.Parse(csvContent);
        var result = _materialService.ImportPriceList(rows);

        var messages = new List<string>();

        if (result.UpdatedCount > 0)
        {
            messages.Add(result.UpdatedCount == 1
                ? "Updated 1 price."
                : $"Updated {result.UpdatedCount} prices.");
        }

        if (result.AddedNames.Count > 0)
        {
            messages.Add(result.AddedNames.Count == 1
                ? $"Added 1 new material (\"{result.AddedNames[0]}\")."
                : $"Added {result.AddedNames.Count} new materials ({string.Join(", ", result.AddedNames)}).");
        }

        if (skippedLines.Count > 0)
        {
            messages.Add(skippedLines.Count == 1
                ? "1 line couldn't be read."
                : $"{skippedLines.Count} lines couldn't be read.");
        }

        _materialStatusLabel.Text = messages.Count == 0
            ? "No prices found in that file."
            : string.Join(" ", messages);
    }

    private void OpenAddGemDialog()
    {
        var dialog = new GemEditorDialog();
        var result = dialog.ShowModal(this);

        if (result is null)
            return;

        _gemstoneService.AddGemstone(result);

        _gemStatusLabel.Text = $"Added \"{result.Name}\".";
    }

    private void SyncDefaultGems()
    {
        var addedCount = _gemstoneService.AddMissingDefaults();
        var refreshedCount = _gemstoneService.RefreshDefaultAppearance();

        var messages = new List<string>();

        if (addedCount > 0)
        {
            messages.Add(addedCount == 1 ? "Added 1 default gem." : $"Added {addedCount} default gems.");
        }

        if (refreshedCount > 0)
        {
            messages.Add(refreshedCount == 1
                ? "Refreshed 1 gem's appearance."
                : $"Refreshed {refreshedCount} gems' appearance.");
        }

        _gemStatusLabel.Text = messages.Count == 0
            ? "Default gems are already in sync."
            : string.Join(" ", messages);
    }
}
