using Eto.Drawing;
using Eto.Forms;
using Piantina.Core.Gems;
using Piantina.Plugin.Controls;
using Piantina.Plugin.UI;

namespace Piantina.Plugin.Views.Gems;

/// <summary>
/// Mirrors MaterialList's tile-grid layout and search/select/refresh
/// behaviour. Grouping is simpler than materials: gems don't have a "24ct"-
/// style name prefix to derive a group from, so tiles are grouped directly
/// by GemCategory (Diamond, Ruby, Sapphire, ...).
/// </summary>
public class GemList : Card
{
    private const int Columns = 3;

    public event Action<Gemstone>? GemSelected;

    public event Action<Gemstone>? GemDoubleClicked;

    private readonly GemstoneService _service;
    private readonly PiantinaSearchBox _searchBox;
    private readonly Panel _gemHost;
    private GemListItem? _selectedItem;

    private Guid? _selectedGemId;

    public GemList(GemstoneService service)
        : base("Gems")
    {
        _service = service;

        _searchBox = new PiantinaSearchBox();
        _searchBox.TextChanged += SearchBox_TextChanged;

        _gemHost = new Panel();

        WithContent(
            _searchBox,
            _gemHost);

        BuildGemList();
    }

    /// <summary>
    /// Rebuilds the list from the current state of the service, preserving the
    /// active search filter. Pass a gem Id (e.g. one just added or edited)
    /// to select it after rebuilding.
    /// </summary>
    public void Refresh(Guid? selectGemId = null)
    {
        if (selectGemId.HasValue)
            _selectedGemId = selectGemId;

        BuildGemList(_searchBox.Text);
    }

    private void BuildGemList(string searchText = "")
    {
        _selectedItem = null;

        searchText = searchText.Trim();

        var layout = new DynamicLayout
        {
            Spacing = new Size(0, 12)
        };

        var matchingGems = _service.GetGemstones()
            .Where(gemstone => gemstone.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var groups = matchingGems
            .GroupBy(gemstone => gemstone.Category)
            .OrderBy(group => group.Min(gemstone => gemstone.SortOrder));

        foreach (var group in groups)
        {
            var gemstones = group
                .OrderBy(gemstone => gemstone.SortOrder)
                .ThenBy(gemstone => gemstone.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var groupLabel = new Label
            {
                Text = group.Key.ToString(),
                Font = AppFonts.Heading,
                TextColor = AppColors.Text
            };

            layout.AddRow(groupLabel);

            var grid = new TableLayout
            {
                Padding = 0,
                Spacing = new Size(8, 8)
            };

            for (var i = 0; i < gemstones.Count; i += Columns)
            {
                var row = new TableRow();

                for (var column = 0; column < Columns; column++)
                {
                    if (i + column >= gemstones.Count)
                    {
                        row.Cells.Add(null);
                        continue;
                    }

                    var gemstone = gemstones[i + column];
                    var item = new GemListItem(gemstone);

                    item.Selected += Gem_Selected;
                    item.DoubleClicked += Gem_DoubleClicked;

                    if (_selectedGemId == gemstone.Id)
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

        _gemHost.Content = layout;
    }

    private void SearchBox_TextChanged(object? sender, EventArgs e)
    {
        BuildGemList(_searchBox.Text);
    }

    private void Gem_Selected(object? sender, EventArgs e)
    {
        if (sender is not GemListItem item)
            return;

        _selectedItem?.Deselect();

        item.Select();

        _selectedItem = item;
        _selectedGemId = item.Gemstone.Id;

        GemSelected?.Invoke(item.Gemstone);
    }

    /// <summary>
    /// Selects the double-clicked tile the same way a single click would (in
    /// case double-click fires without a preceding single-click selection on
    /// some platform), then raises GemDoubleClicked after GemSelected has
    /// already run - so a subscriber can rely on GemDetails already showing
    /// this gem by the time it acts on GemDoubleClicked.
    /// </summary>
    private void Gem_DoubleClicked(object? sender, EventArgs e)
    {
        if (sender is not GemListItem item)
            return;

        Gem_Selected(sender, e);

        GemDoubleClicked?.Invoke(item.Gemstone);
    }
}
