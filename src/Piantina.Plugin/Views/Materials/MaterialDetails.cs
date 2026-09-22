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
    private readonly Button _deactivateButton;

    private Material? _material;

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

        _finishSelector = new RadioButtonList
        {
            Orientation = Orientation.Horizontal,
            Spacing = new Size(10, 0),
            Items = { "Polish", "Hammered", "Sand Blast" }
        };
        _finishSelector.SelectedIndex = 0;

        // Label and selector share one row rather than the selector getting
        // its own heading row above it - saves vertical space now that this
        // card is pinned at the bottom of the Materials tab.
        var finishRow = new TableLayout { Padding = 0, Spacing = new Size(10, 4) };
        var finishTableRow = new TableRow();
        finishTableRow.Cells.Add(new TableCell(
            new Label { Text = "Finish", Font = AppFonts.Body, TextColor = AppColors.TextMuted },
            false));
        finishTableRow.Cells.Add(new TableCell(_finishSelector, true));
        finishRow.Rows.Add(finishTableRow);

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

        _deactivateButton = new Button { Text = "Deactivate" };
        _deactivateButton.Click += (_, _) =>
        {
            if (_material is not null)
                DeactivateRequested?.Invoke(_material);
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

    public void ShowMaterial(Material material)
    {
        _material = material;

        _name.Value = material.Name;
        _category.Value = material.Category.ToString();
        _density.Value = material.Density.ToString("0.00");
        _price.Value = $"R {material.PricePerGram:N2}";
        _notes.Value = string.IsNullOrWhiteSpace(material.Notes)
            ? "-"
            : material.Notes;

        _colorSwatch.BackgroundColor = Color.FromArgb(material.ColorR, material.ColorG, material.ColorB);
        _applyStatusLabel.Text = string.Empty;

        SetButtonsEnabled(true);
    }

    /// <summary>
    /// Resets the panel to its empty state. Used after the shown material is
    /// deactivated (and so disappears from the list).
    /// </summary>
    public void Clear()
    {
        _material = null;

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
    /// rather than a separate Material Library panel.
    /// </summary>
    private void ApplyToSelection()
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
        _deactivateButton.Enabled = enabled;
        _applyButton.Enabled = enabled;
    }
}
