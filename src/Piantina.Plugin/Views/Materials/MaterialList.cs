using Piantina.Core.Materials;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Materials;



public class MaterialList : Card
{
    public event Action<Material>? MaterialSelected;

    private MaterialListItem? _selectedItem;

    public MaterialList()
        : base("Materials")
    {
        Width = 300;

        var service = new MaterialService();

        foreach (var material in service.GetMaterials())
        {
            var item = new MaterialListItem(material);

            item.Selected += Material_Selected;

            WithContent(item);
        }
    }

    private void Material_Selected(object? sender, EventArgs e)
    {
        if (sender is not MaterialListItem item)
            return;

        _selectedItem?.Deselect();

        item.Select();

        _selectedItem = item;

        MaterialSelected?.Invoke(item.Material);
    }
}