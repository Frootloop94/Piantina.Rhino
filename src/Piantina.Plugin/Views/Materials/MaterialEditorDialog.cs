using System;
using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Materials;

/// <summary>
/// Add/Edit dialog for a Material. Pass an existing Material to edit it in place;
/// pass null (or omit) to create a new one.
/// Returns the resulting Material via ShowModal(), or null if cancelled.
/// </summary>
public class MaterialEditorDialog : Dialog<Material?>
{
    private readonly TextBox _nameTextBox;
    private readonly DropDown _categoryDropDown;
    private readonly NumericStepper _densityStepper;
    private readonly NumericStepper _priceStepper;
    private readonly ColorPicker _colorPicker;
    private readonly NumericStepper _reflectivityStepper;
    private readonly NumericStepper _shineStepper;
    private readonly TextArea _notesTextArea;
    private readonly Label _errorLabel;

    private readonly Material? _existing;

    public MaterialEditorDialog(Material? existing = null)
    {
        _existing = existing;

        Title = existing is null ? "Add Material" : "Edit Material";
        ClientSize = new Size(420, 620);
        Resizable = false;
        Padding = new Padding(24);

        _nameTextBox = new TextBox
        {
            Font = AppFonts.Body,
            Text = existing?.Name ?? string.Empty
        };

        _categoryDropDown = new DropDown
        {
            Font = AppFonts.Body
        };

        foreach (MaterialCategory category in Enum.GetValues<MaterialCategory>())
        {
            _categoryDropDown.Items.Add(new ListItem
            {
                Text = category.ToString(),
                Key = category.ToString()
            });
        }

        _categoryDropDown.SelectedKey = (existing?.Category ?? MaterialCategory.Gold).ToString();

        _densityStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 100,
            Increment = 0.01,
            Value = (double)(existing?.Density ?? 0m)
        };

        _priceStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 1_000_000,
            Increment = 1,
            Value = (double)(existing?.PricePerGram ?? 0m)
        };

        _colorPicker = new ColorPicker
        {
            Value = Color.FromArgb(
                existing?.ColorR ?? 200,
                existing?.ColorG ?? 200,
                existing?.ColorB ?? 200)
        };

        _reflectivityStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 1,
            Increment = 0.05,
            Value = existing?.Reflectivity ?? 0.5
        };

        _shineStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 1,
            Increment = 0.05,
            Value = existing?.Shine ?? 0.6
        };

        _notesTextArea = new TextArea
        {
            Font = AppFonts.Body,
            Height = 70,
            Text = existing?.Notes ?? string.Empty
        };

        _errorLabel = new Label
        {
            TextColor = Colors.Crimson,
            Font = AppFonts.Body,
            Visible = false
        };

        var saveButton = new PrimaryButton(
            existing is null ? "Add Material" : "Save Changes",
            Save);

        var cancelButton = new Button { Text = "Cancel" };
        cancelButton.Click += (_, _) => Close(null);

        DefaultButton = saveButton;
        AbortButton = cancelButton;

        var form = new DynamicLayout
        {
            Spacing = new Size(6, 10)
        };

        AddField(form, "Name", _nameTextBox);
        AddField(form, "Category", _categoryDropDown);
        AddField(form, "Density (g/cm³)", _densityStepper);
        AddField(form, "Price per gram", _priceStepper);
        AddField(form, "Appearance colour", _colorPicker);
        AddField(form, "Reflectivity", _reflectivityStepper);
        AddField(form, "Shine", _shineStepper);
        AddField(form, "Notes", _notesTextArea);

        form.AddRow(_errorLabel);

        var buttonRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Items = { cancelButton, saveButton }
        };

        Content = new StackLayout
        {
            Spacing = 20,
            Items =
            {
                new StackLayoutItem(form, true),
                new StackLayoutItem(buttonRow, HorizontalAlignment.Right)
            }
        };
    }

    private static void AddField(DynamicLayout form, string label, Control control)
    {
        form.AddRow(new Label
        {
            Text = label,
            Font = AppFonts.Body,
            TextColor = AppColors.TextMuted
        });

        form.AddRow(control);
    }

    private void Save()
    {
        var name = _nameTextBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("Name is required.");
            return;
        }

        if (_categoryDropDown.SelectedKey is null ||
            !Enum.TryParse<MaterialCategory>(_categoryDropDown.SelectedKey, out var category))
        {
            ShowError("Please select a category.");
            return;
        }

        if (_densityStepper.Value <= 0)
        {
            ShowError("Density must be greater than zero.");
            return;
        }

        var material = _existing ?? new Material();

        material.Name = name;
        material.Category = category;
        material.Density = (decimal)_densityStepper.Value;
        material.PricePerGram = (decimal)_priceStepper.Value;
        material.ColorR = (byte)Math.Round(_colorPicker.Value.R * 255f);
        material.ColorG = (byte)Math.Round(_colorPicker.Value.G * 255f);
        material.ColorB = (byte)Math.Round(_colorPicker.Value.B * 255f);
        material.Reflectivity = _reflectivityStepper.Value;
        material.Shine = _shineStepper.Value;
        material.Notes = _notesTextArea.Text?.Trim() ?? string.Empty;

        Close(material);
    }

    private void ShowError(string message)
    {
        _errorLabel.Text = message;
        _errorLabel.Visible = true;
    }
}
