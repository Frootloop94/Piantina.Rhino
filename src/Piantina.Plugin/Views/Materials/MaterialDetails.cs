using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Materials;

public class MaterialDetails : Card
{
    public event Action<Material>? EditRequested;
    public event Action<Material>? DeactivateRequested;

    private readonly InfoRow _name;
    private readonly InfoRow _category;
    private readonly InfoRow _density;
    private readonly InfoRow _price;
    private readonly InfoRow _notes;

    private readonly PrimaryButton _editButton;
    private readonly Button _deactivateButton;

    private Material? _material;

    public MaterialDetails()
        : base("Material Details")
    {
        _name = new InfoRow("Name", "-");
        _category = new InfoRow("Category", "-");
        _density = new InfoRow("Density", "-");
        _price = new InfoRow("Price", "-");
        _notes = new InfoRow("Notes", "-");

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

        SetButtonsEnabled(false);
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _editButton.Enabled = enabled;
        _deactivateButton.Enabled = enabled;
    }
}
