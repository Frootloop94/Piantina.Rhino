using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.Controls;

namespace Piantina.Plugin.Views.Materials;

public class MaterialsView : Panel
{
    public MaterialsView()
    {
        var materialList = new MaterialList();

        var materialDetails = new MaterialDetails();

        var layout = new TableLayout
        {
            Spacing = new Size(20, 20),

            Rows =
        {
        new TableRow(
            new TableCell(materialList, false),
            new TableCell(materialDetails, true))
        }
        };

        materialList.MaterialSelected += materialDetails.ShowMaterial;

        Content = new StackLayout
        {
            Spacing = 30,

            Items =
    {
        new SectionHeader("Materials"),
        layout
    }
        };
    }
}