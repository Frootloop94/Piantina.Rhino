using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Materials;

public class MaterialList : Card
{
    public event Action<Material>? MaterialSelected;

    private readonly MaterialService _service;
    private readonly PiantinaSearchBox _searchBox;
    private readonly Panel _materialHost;
    private MaterialListItem? _selectedItem;

    private Guid? _selectedMaterialId;

    public MaterialList(MaterialService service)
    : base("Materials")
    {
        Width = 300;

        _service = service;

        _searchBox = new PiantinaSearchBox();
        _searchBox.TextChanged += SearchBox_TextChanged;

        _materialHost = new Panel();

        WithContent(
            _searchBox,
            _materialHost);

        BuildMaterialList();
    }

    /// <summary>
    /// Rebuilds the list from the current state of the service, preserving the
    /// active search filter. Pass a material Id (e.g. one just added or edited)
    /// to select it after rebuilding.
    /// </summary>
    public void Refresh(Guid? selectMaterialId = null)
    {
        if (selectMaterialId.HasValue)
            _selectedMaterialId = selectMaterialId;

        BuildMaterialList(_searchBox.Text);
    }

    private void BuildMaterialList(string searchText = "")
    {
        _selectedItem = null;

        searchText = searchText.Trim();

        var layout = new DynamicLayout
        {
            Spacing = new Eto.Drawing.Size(0, 8)
        };

        // Sorted so the list has a stable, predictable order regardless of the
        // order materials were added/edited in - alphabetical by category, then
        // alphabetical by name within each category.
        var sortedGroups = _service.GetMaterialsByCategory()
            .OrderBy(group => group.Key.ToString(), StringComparer.OrdinalIgnoreCase);

        foreach (var group in sortedGroups)
        {
            var matchingMaterials = group
                .Where(material =>
                    material.Name.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(material => material.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (matchingMaterials.Count == 0)
                continue;

            var categoryLabel = new Label
            {
                Text = group.Key.ToString(),
                Font = AppFonts.Heading,
                TextColor = AppColors.Text
            };

            layout.AddRow(categoryLabel);

            foreach (var material in matchingMaterials)
            {
                var item = new MaterialListItem(material);

                item.Selected += Material_Selected;

                if (_selectedMaterialId == material.Id)
                {
                    item.Select();
                    _selectedItem = item;
                }

                layout.AddRow(item);
            }
        }

        _materialHost.Content = layout;
    }


    private void SearchBox_TextChanged(object? sender, EventArgs e)
    {
        BuildMaterialList(_searchBox.Text);
    }

    private void Material_Selected(object? sender, EventArgs e)
    {
        if (sender is not MaterialListItem item)
            return;

        _selectedItem?.Deselect();

        item.Select();

        _selectedItem = item;
        _selectedMaterialId = item.Material.Id;

        MaterialSelected?.Invoke(item.Material);
    }
}
