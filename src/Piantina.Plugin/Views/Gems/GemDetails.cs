using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Gems;
using Piantina.Plugin.Controls;
using Piantina.Plugin.Services;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Gems;

public class GemDetails : Card
{
    public event Action<Gemstone>? EditRequested;
    public event Action<Gemstone>? DeactivateRequested;

    private readonly InfoRow _name;
    private readonly InfoRow _category;
    private readonly InfoRow _transparency;
    private readonly InfoRow _refractiveIndex;
    private readonly Panel _colorSwatch;
    private readonly PrimaryButton _applyButton;
    private readonly Label _applyStatusLabel;
    private readonly InfoRow _notes;

    private readonly PrimaryButton _editButton;
    private readonly Button _deactivateButton;

    private Gemstone? _gemstone;

    public GemDetails()
        : base("Gem Details", padding: 14, contentSpacing: 4, headerSpacing: 10)
    {
        _name = new InfoRow("Name", "-");
        _category = new InfoRow("Category", "-");
        _transparency = new InfoRow("Transparency", "-");
        _refractiveIndex = new InfoRow("Refractive Index", "-");
        _notes = new InfoRow("Notes", "-");

        _colorSwatch = new Panel
        {
            Width = 40,
            Height = 20,
            BackgroundColor = Colors.Transparent
        };

        var colorRow = new TableLayout { Padding = 0, Spacing = new Size(10, 4) };
        var colorTableRow = new TableRow();
        colorTableRow.Cells.Add(new TableCell(
            new Label { Text = "Appearance", Font = AppFonts.Body, TextColor = AppColors.TextMuted },
            true));
        colorTableRow.Cells.Add(new TableCell(_colorSwatch, false));
        colorRow.Rows.Add(colorTableRow);

        // No Metal Finish selector here - polish/hammered/sand blast is a
        // metal surface-treatment concept that doesn't apply to a faceted gem.
        _applyButton = new PrimaryButton("Apply to Selection", ApplyToSelection);

        _applyStatusLabel = new Label
        {
            Font = AppFonts.Small,
            TextColor = AppColors.TextMuted
        };

        _editButton = new PrimaryButton("Edit", () =>
        {
            if (_gemstone is not null)
                EditRequested?.Invoke(_gemstone);
        });

        _deactivateButton = new Button { Text = "Deactivate" };
        _deactivateButton.Click += (_, _) =>
        {
            if (_gemstone is not null)
                DeactivateRequested?.Invoke(_gemstone);
        };

        var buttonRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Items = { _editButton, _deactivateButton }
        };

        WithContent(
            _name,
            _category,
            _transparency,
            _refractiveIndex,
            colorRow,
            _applyButton,
            _applyStatusLabel,
            _notes,
            buttonRow);

        SetButtonsEnabled(false);
    }

    public void ShowGem(Gemstone gemstone)
    {
        _gemstone = gemstone;

        _name.Value = gemstone.Name;
        _category.Value = gemstone.Category.ToString();
        _transparency.Value = gemstone.Transparency.ToString("0.00");
        _refractiveIndex.Value = gemstone.RefractiveIndex.ToString("0.00");
        _notes.Value = string.IsNullOrWhiteSpace(gemstone.Notes)
            ? "-"
            : gemstone.Notes;

        _colorSwatch.BackgroundColor = Color.FromArgb(gemstone.ColorR, gemstone.ColorG, gemstone.ColorB);
        _applyStatusLabel.Text = string.Empty;

        SetButtonsEnabled(true);
    }

    /// <summary>
    /// Resets the panel to its empty state. Used after the shown gem is
    /// deactivated (and so disappears from the list).
    /// </summary>
    public void Clear()
    {
        _gemstone = null;

        _name.Value = "-";
        _category.Value = "-";
        _transparency.Value = "-";
        _refractiveIndex.Value = "-";
        _notes.Value = "-";

        _colorSwatch.BackgroundColor = Colors.Transparent;
        _applyStatusLabel.Text = string.Empty;

        SetButtonsEnabled(false);
    }

    /// <summary>
    /// Applies the shown gem to whatever's currently selected in the Rhino
    /// viewport. Public so GemList's double-click can trigger it too, not
    /// just the Apply button here.
    /// </summary>
    public void ApplyToSelection()
    {
        if (_gemstone is null)
            return;

        var result = GemAppearanceService.ApplyToSelection(_gemstone);

        _applyStatusLabel.Text = result switch
        {
            GemAppearanceService.ApplyResult.Applied =>
                $"Applied {_gemstone.Name} to the selection.",
            GemAppearanceService.ApplyResult.NothingSelected =>
                "Select objects in Rhino first, then click Apply.",
            _ => "No active Rhino document."
        };
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _editButton.Enabled = enabled;
        _deactivateButton.Enabled = enabled;
        _applyButton.Enabled = enabled;
    }
}
