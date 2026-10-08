using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.Services;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Materials;

public class MaterialDetails : Card
{
    public event Action<Material>? EditRequested;
    public event Action<Material>? DeactivateRequested;
    public event Action<Material>? ResetToDefaultRequested;

    private readonly InfoRow _name;
    private readonly InfoRow _category;
    private readonly InfoRow _density;
    private readonly InfoRow _price;
    private readonly Panel _colorSwatch;
    private readonly RadioButtonList _finishSelector;
    private readonly PrimaryButton _applyButton;
    private readonly Label _applyStatusLabel;
    private readonly InfoRow _notes;

    private readonly PrimaryButton _editButton;
    private readonly Button _resetButton;
    private readonly Button _deactivateButton;

    private Material? _material;
    private bool _hasDefault;

    public MaterialDetails()
        : base("Material Details", padding: 14, contentSpacing: 4, headerSpacing: 10)
    {
        _name = new InfoRow("Name", "-");
        _category = new InfoRow("Category", "-");
        _density = new InfoRow("Density", "-");
        _price = new InfoRow("Price", "-");
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

        // Vertical, not horizontal - "Polish"/"Hammered"/"Sand Blast" side by
        // side need more width than a narrow docked sidebar has, which was
        // forcing this card to clip or scroll horizontally.
        _finishSelector = new RadioButtonList
        {
            Orientation = Orientation.Vertical,
            Spacing = new Size(0, 2),
            Items = { "Polish", "Hammered", "Sand Blast" }
        };
        _finishSelector.SelectedIndex = 0;

        var finishRow = new StackLayout
        {
            Spacing = 4,
            Items =
            {
                new Label { Text = "Finish", Font = AppFonts.Body, TextColor = AppColors.TextMuted },
                _finishSelector
            }
        };

        _applyButton = new PrimaryButton("Apply to Selection", ApplyToSelection);

        _applyStatusLabel = new Label
        {
            Font = AppFonts.Small,
            TextColor = AppColors.TextMuted
        };

        _editButton = new PrimaryButton("Edit", () =>
        {
            if (_material is not null)
                EditRequested?.Invoke(_material);
        });

        _resetButton = new Button { Text = "Reset to Default" };
        _resetButton.Click += (_, _) =>
        {
            if (_material is not null)
                ResetToDefaultRequested?.Invoke(_material);
        };

        _deactivateButton = new Button { Text = "Deactivate" };
        _deactivateButton.Click += (_, _) =>
        {
            if (_material is not null)
                DeactivateRequested?.Invoke(_material);
        };

        // Vertical, not side by side - Edit/Reset to Default/Deactivate
        // together need more width than a narrow docked sidebar has.
        var buttonRow = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Items = { _editButton, _resetButton, _deactivateButton }
        };

        WithContent(
            _name,
            _category,
            _density,
            _price,
            colorRow,
            finishRow,
            _applyButton,
            _applyStatusLabel,
            _notes,
            buttonRow);

        SetButtonsEnabled(false);
    }

    /// <summary>
    /// hasDefault controls whether "Reset to Default" is enabled - there's
    /// nothing to reset a custom (or renamed) material back to. statusMessage
    /// overrides the usual blank status line, e.g. to confirm a reset just
    /// happened, since that also comes through here to refresh the displayed
    /// fields.
    /// </summary>
    public void ShowMaterial(Material material, bool hasDefault, string? statusMessage = null)
    {
        _material = material;
        _hasDefault = hasDefault;

        _name.Value = material.Name;
        _category.Value = material.Category.ToString();
        _density.Value = material.Density.ToString("0.00");
        _price.Value = $"R {material.PricePerGram:N2}";
        _notes.Value = string.IsNullOrWhiteSpace(material.Notes)
            ? "-"
            : material.Notes;

        _colorSwatch.BackgroundColor = Color.FromArgb(material.ColorR, material.ColorG, material.ColorB);
        _applyStatusLabel.Text = statusMessage ?? string.Empty;

        SetButtonsEnabled(true);
    }

    /// <summary>
    /// Resets the panel to its empty state. Used after the shown material is
    /// deactivated (and so disappears from the list).
    /// </summary>
    public void Clear()
    {
        _material = null;
        _hasDefault = false;

        _name.Value = "-";
        _category.Value = "-";
        _density.Value = "-";
        _price.Value = "-";
        _notes.Value = "-";

        _colorSwatch.BackgroundColor = Colors.Transparent;
        _applyStatusLabel.Text = string.Empty;

        SetButtonsEnabled(false);
    }

    /// <summary>
    /// Applies the shown material, with the chosen finish, to whatever's
    /// currently selected in the Rhino viewport - the material catalog and
    /// "give my selection this look" action now live on the same screen,
    /// rather than a separate Material Library panel. Public so MaterialsView
    /// can also trigger it from a double-click on a tile in the list, not
    /// just the Apply button here.
    /// </summary>
    public void ApplyToSelection()
    {
        if (_material is null)
            return;

        var finish = _finishSelector.SelectedIndex switch
        {
            1 => MetalFinish.Hammered,
            2 => MetalFinish.SandBlast,
            _ => MetalFinish.Polished
        };

        var result = MaterialAppearanceService.ApplyToSelection(_material, finish);

        _applyStatusLabel.Text = result switch
        {
            MaterialAppearanceService.ApplyResult.Applied =>
                $"Applied {_material.Name} ({finish}) to the selection.",
            MaterialAppearanceService.ApplyResult.NothingSelected =>
                "Select objects in Rhino first, then click Apply.",
            _ => "No active Rhino document."
        };
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _editButton.Enabled = enabled;
        _resetButton.Enabled = enabled && _hasDefault;
        _deactivateButton.Enabled = enabled;
        _applyButton.Enabled = enabled;
    }
}
