using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Materials;

public class MaterialList : Card
{
    // Matches MaterialListItem's own tile Width plus the grid's column
    // spacing, so column count can be derived from however much width is
    // actually available rather than a fixed guess - a narrow docked panel
    // gets fewer, taller columns instead of clipping a column that doesn't
    // fit (what a hardcoded column count used to do).
    private const int TileWidth = MaterialListItem.TileWidth;
    private const int GridSpacing = 8;
    private const int MinColumns = 1;

    // 2, not 3 - the live-width plumbing (SetAvailableWidth, wired up from
    // PiantinaPanel's resize) hasn't proven reliable inside Rhino's docked-
    // panel host, so this is effectively the number that's actually in play
    // most of the time, not just a pre-layout placeholder. 2 columns of
    // TileWidth fit comfortably even at a narrow docked width, where 3
    // routinely clipped the third tile.
    private const int FallbackColumns = 2;

    public event Action<Material>? MaterialSelected;

    public event Action<Material>? MaterialDoubleClicked;

    private readonly MaterialService _service;
    private readonly PiantinaSearchBox _searchBox;
    private readonly Panel _materialHost;
    private MaterialListItem? _selectedItem;

    private Guid? _selectedMaterialId;

    // Set from outside (MaterialsView, from its wrapping Scrollable's
    // SizeChanged) rather than read from _materialHost.Width - a plain,
    // unstretched Panel's own Width getter doesn't reliably reflect its
    // rendered size in Eto, so it never shrank below FallbackColumns no
    // matter how narrow the docked panel actually was. A Scrollable's
    // ClientSize is the real, dependable source of the visible viewport
    // width.
    private int _availableWidth;

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
    /// Called by MaterialsView whenever the scrollable region hosting this
    /// list resizes, so the grid can re-flow its column count to match.
    /// </summary>
    public void SetAvailableWidth(int width)
    {
        if (width == _availableWidth)
            return;

        _availableWidth = width;

        BuildMaterialList(_searchBox.Text);
    }

    /// <summary>
    /// How many tiles fit per row at the last known available width. Falls
    /// back to a sane default before MaterialsView has reported a real width
    /// (e.g. during construction, before the first layout pass).
    /// </summary>
    private int ComputeColumns()
    {
        if (_availableWidth <= 0)
        {
            return FallbackColumns;
        }

        var columns = (_availableWidth + GridSpacing) / (TileWidth + GridSpacing);

        return Math.Max(MinColumns, columns);
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

        var columns = ComputeColumns();

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
                Spacing = new Size(GridSpacing, GridSpacing)
            };

            for (var i = 0; i < materials.Count; i += columns)
            {
                var row = new TableRow();

                for (var column = 0; column < columns; column++)
                {
                    if (i + column >= materials.Count)
                    {
                        row.Cells.Add(null);
                        continue;
                    }

                    var material = materials[i + column];
                    var item = new MaterialListItem(material);

                    item.Selected += Material_Selected;
                    item.DoubleClicked += Material_DoubleClicked;

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

    /// <summary>
    /// Selects the double-clicked tile the same way a single click would
    /// (in case double-click fires without a preceding single-click
    /// selection on some platforms), then raises MaterialDoubleClicked after
    /// MaterialSelected has already run - so by the time a subscriber acts on
    /// MaterialDoubleClicked, MaterialDetails is already showing this material.
    /// </summary>
    private void Material_DoubleClicked(object? sender, EventArgs e)
    {
        if (sender is not MaterialListItem item)
            return;

        Material_Selected(sender, e);

        MaterialDoubleClicked?.Invoke(item.Material);
    }
}
