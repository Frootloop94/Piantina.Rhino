using Piantina.Core.Materials;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Materials;

public class MaterialDetails : Card
{
    private readonly InfoRow _name;
    private readonly InfoRow _category;
    private readonly InfoRow _density;
    private readonly InfoRow _price;
    private readonly InfoRow _notes;

    public MaterialDetails()
        : base("Material Details")
    {
        _name = new InfoRow("Name", "-");
        _category = new InfoRow("Category", "-");
        _density = new InfoRow("Density", "-");
        _price = new InfoRow("Price", "-");
        _notes = new InfoRow("Notes", "-");

        WithContent(
            _name,
            _category,
            _density,
            _price,
            _notes);
    }
    public void ShowMaterial(Material material)
    {
        _name.Value = material.Name;
        _category.Value = material.Category.ToString();
        _density.Value = material.Density.ToString("0.00");
        _price.Value = $"R {material.PricePerGram:N2}";
        _notes.Value = string.IsNullOrWhiteSpace(material.Notes)
            ? "-"
            : material.Notes;
    }
}
