using Eto.Forms;
using Piantina.Core.Gems;
using Piantina.Core.Materials;
using Piantina.Plugin.Controls;
using Piantina.Plugin.Views.Gems;

namespace Piantina.Plugin.Views.Materials;

public class MaterialsView : Panel
{
    private readonly MaterialService _materialService;
    private readonly MaterialList _materialList;
    private readonly MaterialDetails _materialDetails;

    private readonly GemstoneService _gemstoneService;
    private readonly GemList _gemList;
    private readonly GemDetails _gemDetails;

    public MaterialsView()
    {
        Padding = 20;

        _materialService = new MaterialService();

        _materialList = new MaterialList(_materialService);
        _materialDetails = new MaterialDetails();

        _materialList.MaterialSelected += _materialDetails.ShowMaterial;
        _materialList.MaterialDoubleClicked += _ => _materialDetails.ApplyToSelection();
        _materialDetails.EditRequested += OpenEditMaterialDialog;
        _materialDetails.DeactivateRequested += DeactivateMaterial;

        _gemstoneService = new GemstoneService();

        _gemList = new GemList(_gemstoneService);
        _gemDetails = new GemDetails();

        _gemList.GemSelected += _gemDetails.ShowGem;
        _gemList.GemDoubleClicked += _ => _gemDetails.ApplyToSelection();
        _gemDetails.EditRequested += OpenEditGemDialog;
        _gemDetails.DeactivateRequested += DeactivateGem;

        // Metal and Gems as sub-tabs rather than one combined catalog - gems
        // don't have pricing or a karat-style grouping, so mixing them into
        // the same list/details pair would mean a lot of "N/A" fields either
        // way.
        var tabControl = new TabControl();

        tabControl.Pages.Add(new TabPage
        {
            Text = "Metal",
            Content = BuildSection(_materialList, _materialDetails)
        });

        tabControl.Pages.Add(new TabPage
        {
            Text = "Gems",
            Content = BuildSection(_gemList, _gemDetails)
        });

        Content = new StackLayout
        {
            Spacing = 12,

            Items =
            {
                new SectionHeader("Materials"),
                new StackLayoutItem(tabControl, true)
            }
        };
    }

    /// <summary>
    /// The list scrolls in its own region rather than the whole tab scrolling
    /// as one long page, so Details stays pinned and visible at the bottom
    /// instead of getting scrolled out of view. Same layout for both the
    /// Metal and Gems sub-tabs.
    /// </summary>
    private static Control BuildSection(Control list, Control details)
    {
        var scrollableList = new Scrollable
        {
            Content = list,
            Border = BorderType.None
        };

        return new StackLayout
        {
            Spacing = 16,

            Items =
            {
                new StackLayoutItem(scrollableList, true),
                details
            }
        };
    }

    /// <summary>
    /// Refreshes both lists from the current state of their services. Called
    /// by PiantinaPanel when this tab is selected, since adding a material or
    /// gem now happens on the Settings tab and wouldn't otherwise be
    /// reflected here until this view was rebuilt from scratch.
    /// </summary>
    public void Refresh()
    {
        _materialList.Refresh();
        _gemList.Refresh();
    }

    /// <summary>
    /// Because Material is a reference type, editing mutates the same instance
    /// the list and details panel already hold, so refreshing after Save is
    /// enough - there's nothing to re-fetch.
    /// </summary>
    private void OpenEditMaterialDialog(Material existing)
    {
        var dialog = new MaterialEditorDialog(existing);
        var result = dialog.ShowModal(this);

        if (result is null)
            return;

        _materialService.NotifyMaterialUpdated();

        _materialList.Refresh(result.Id);
        _materialDetails.ShowMaterial(result);
    }

    private void DeactivateMaterial(Material material)
    {
        _materialService.SetActive(material.Id, false);

        _materialList.Refresh();
        _materialDetails.Clear();
    }

    private void OpenEditGemDialog(Gemstone existing)
    {
        var dialog = new GemEditorDialog(existing);
        var result = dialog.ShowModal(this);

        if (result is null)
            return;

        _gemstoneService.NotifyGemstoneUpdated();

        _gemList.Refresh(result.Id);
        _gemDetails.ShowGem(result);
    }

    private void DeactivateGem(Gemstone gemstone)
    {
        _gemstoneService.SetActive(gemstone.Id, false);

        _gemList.Refresh();
        _gemDetails.Clear();
    }
}
