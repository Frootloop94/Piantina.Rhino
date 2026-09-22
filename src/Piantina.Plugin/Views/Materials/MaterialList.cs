using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Materials;

public class MaterialList : Card
{
    private const int Columns = 3;

    public event Action<Material>? MaterialSelected;

    private readonly MaterialService _service;
    private readonly PiantinaSearchBox _searchBox;
    private readonly Panel _materialHost;
    private MaterialListItem? _selectedItem;

    private Guid? _selectedMaterialId;

    public MaterialList(MaterialService service)
    : base("Materials")
    {
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
            Spacing = new Size(0, 12)
        };

        var matchingMaterials = _service.GetMaterials()
            .Where(material => material.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Grouped by karat (e.g. "24ct", "18ct") rather than MaterialCategory,
        // so the different golds tile together the way the business's own
        // metal list is organised - tiles side by side like RhinoGold's old
        // material palette, not a plain vertical list. Falls back to the
        // material's Category for anything without a "<N>ct" name prefix
        // (the silvers, and any future non-gold material). Groups are ordered
        // by their members' own SortOrder, which the default catalog already
        // assigns in 24ct-down-to-9ct-then-silver order.
        var groups = matchingMaterials
            .GroupBy(GetGroupLabel)
            .OrderBy(group => group.Min(material => material.SortOrder));

        foreach (var group in groups)
        {
            var materials = group
                .OrderBy(material => material.SortOrder)
                .ThenBy(material => material.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var groupLabel = new Label
            {
                Text = group.Key,
                Font = AppFonts.Heading,
                TextColor = AppColors.Text
            };

            layout.AddRow(groupLabel);

            var grid = new TableLayout
            {
                Padding = 0,
                Spacing = new Size(8, 8)
            };

            for (var i = 0; i < materials.Count; i += Columns)
            {
                var row = new TableRow();

                for (var column = 0; column < Columns; column++)
                {
                    if (i + column >= materials.Count)
                    {
                        row.Cells.Add(null);
                        continue;
                    }

                    var material = materials[i + column];
                    var item = new MaterialListItem(material);

                    item.Selected += Material_Selected;

                    if (_selectedMaterialId == material.Id)
                    {
                        item.Select();
                        _selectedItem = item;
                    }

                    row.Cells.Add(item);
                }

                grid.Rows.Add(row);
            }

            layout.AddRow(grid);
        }

        _materialHost.Content = layout;
    }

    /// <summary>
    /// Returns the leading "&lt;N&gt;ct" token from a material's name (e.g.
    /// "24ct Fine Gold" -> "24ct"), or its Category as a fallback for names
    /// that don't start with a karat (the silvers, or any future non-gold
    /// material).
    /// </summary>
    private static string GetGroupLabel(Material material)
    {
        var name = material.Name;

        var digitCount = 0;
        while (digitCount < name.Length && char.IsDigit(name[digitCount]))
        {
            digitCount++;
        }

        var hasCtSuffix = digitCount > 0
            && name.Length >= digitCount + 2
            && name[digitCount] == 'c'
            && name[digitCount + 1] == 't';

        return hasCtSuffix ? name[..(digitCount + 2)] : material.Category.ToString();
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
