using System;
using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Gems;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Gems;

/// <summary>
/// Add/Edit dialog for a Gemstone. Mirrors MaterialEditorDialog, minus the
/// Density/Price fields (gems aren't priced here - see GemstoneService) and
/// plus Transparency/Refractive Index in their place.
/// </summary>
public class GemEditorDialog : Dialog<Gemstone?>
{
    private readonly TextBox _nameTextBox;
    private readonly DropDown _categoryDropDown;
    private readonly ColorPicker _colorPicker;
    private readonly NumericStepper _transparencyStepper;
    private readonly NumericStepper _refractiveIndexStepper;
    private readonly NumericStepper _reflectivityStepper;
    private readonly NumericStepper _shineStepper;
    private readonly TextArea _notesTextArea;
    private readonly Label _errorLabel;

    private readonly Gemstone? _existing;

    public GemEditorDialog(Gemstone? existing = null)
    {
        _existing = existing;

        Title = existing is null ? "Add Gem" : "Edit Gem";
        ClientSize = new Size(420, 600);
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

        foreach (GemCategory category in Enum.GetValues<GemCategory>())
        {
            _categoryDropDown.Items.Add(new ListItem
            {
                Text = category.ToString(),
                Key = category.ToString()
            });
        }

        _categoryDropDown.SelectedKey = (existing?.Category ?? GemCategory.Diamond).ToString();

        _colorPicker = new ColorPicker
        {
            Value = Color.FromArgb(
                existing?.ColorR ?? 200,
                existing?.ColorG ?? 200,
                existing?.ColorB ?? 200)
        };

        _transparencyStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 1,
            Increment = 0.05,
            Value = existing?.Transparency ?? 0.7
        };

        _refractiveIndexStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 1,
            MaxValue = 3,
            Increment = 0.01,
            Value = existing?.RefractiveIndex ?? 1.5
        };

        _reflectivityStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 1,
            Increment = 0.05,
            Value = existing?.Reflectivity ?? 0.3
        };

        _shineStepper = new NumericStepper
        {
            Font = AppFonts.Body,
            DecimalPlaces = 2,
            MinValue = 0,
            MaxValue = 1,
            Increment = 0.05,
            Value = existing?.Shine ?? 0.9
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
            existing is null ? "Add Gem" : "Save Changes",
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
        AddField(form, "Appearance colour", _colorPicker);
        AddField(form, "Transparency", _transparencyStepper);
        AddField(form, "Refractive index", _refractiveIndexStepper);
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
            !Enum.TryParse<GemCategory>(_categoryDropDown.SelectedKey, out var category))
        {
            ShowError("Please select a category.");
            return;
        }

        var gemstone = _existing ?? new Gemstone();

        gemstone.Name = name;
        gemstone.Category = category;
        gemstone.ColorR = (byte)Math.Round(_colorPicker.Value.R * 255f);
        gemstone.ColorG = (byte)Math.Round(_colorPicker.Value.G * 255f);
        gemstone.ColorB = (byte)Math.Round(_colorPicker.Value.B * 255f);
        gemstone.Transparency = _transparencyStepper.Value;
        gemstone.RefractiveIndex = _refractiveIndexStepper.Value;
        gemstone.Reflectivity = _reflectivityStepper.Value;
        gemstone.Shine = _shineStepper.Value;
        gemstone.Notes = _notesTextArea.Text?.Trim() ?? string.Empty;

        Close(gemstone);
    }

    private void ShowError(string message)
    {
        _errorLabel.Text = message;
        _errorLabel.Visible = true;
    }
}
