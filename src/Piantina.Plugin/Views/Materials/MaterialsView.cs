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

    private readonly Scrollable _materialScrollable;
    private readonly Scrollable _gemScrollable;

    public MaterialsView()
    {
        Padding = 20;

        _materialService = new MaterialService();

        _materialList = new MaterialList(_materialService);
        _materialDetails = new MaterialDetails();

        _materialList.MaterialSelected += material =>
            _materialDetails.ShowMaterial(material, _materialService.HasDefault(material));
        _materialList.MaterialDoubleClicked += _ => _materialDetails.ApplyToSelection();
        _materialDetails.EditRequested += OpenEditMaterialDialog;
        _materialDetails.DeactivateRequested += DeactivateMaterial;
        _materialDetails.ResetToDefaultRequested += ResetMaterialToDefault;

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

        var (metalSection, materialScrollable) = BuildSection(_materialList, _materialDetails);
        _materialScrollable = materialScrollable;

        var (gemSection, gemScrollable) = BuildSection(_gemList, _gemDetails);
        _gemScrollable = gemScrollable;

        _materialScrollable.SizeChanged += (_, _) => UpdateAvailableWidth();
        _gemScrollable.SizeChanged += (_, _) => UpdateAvailableWidth();

        tabControl.Pages.Add(new TabPage { Text = "Metal", Content = metalSection });
        tabControl.Pages.Add(new TabPage { Text = "Gems", Content = gemSection });

        Content = new StackLayout
        {
            Spacing = 12,

            Items =
            {
                new SectionHeader("Materials"),
                new StackLayoutItem(tabControl, true)
            }
        };

        // A Scrollable's own SizeChanged covers most resizes, but on some
        // hosts (Rhino's docked-panel embedding included) it doesn't fire
        // reliably on its own - LoadComplete gives one extra, guaranteed
        // chance to pick up the real width once this view actually has one.
        LoadComplete += (_, _) =>
            Application.Instance.AsyncInvoke(UpdateAvailableWidth);
    }

    /// <summary>
    /// Re-reads both scrollable regions' current viewport width and passes it
    /// to the matching list, so its grid can re-flow its column count. Called
    /// from here on LoadComplete, from each Scrollable's own SizeChanged
    /// below, and from PiantinaPanel whenever the whole docked panel resizes
    /// (the one resize this view is guaranteed to hear about, since Rhino's
    /// docking manager resizes that control directly).
    /// </summary>
    public void UpdateAvailableWidth()
    {
        _materialList.SetAvailableWidth(_materialScrollable.ClientSize.Width);
        _gemList.SetAvailableWidth(_gemScrollable.ClientSize.Width);
    }

    /// <summary>
    /// The list scrolls in its own region rather than the whole tab scrolling
    /// as one long page, so Details stays pinned and visible at the bottom
    /// instead of getting scrolled out of view. Same layout for both the
    /// Metal and Gems sub-tabs. Returns the Scrollable alongside the built
    /// section so the caller can read its ClientSize later (UpdateAvailableWidth)
    /// and listen for its own resizes.
    /// </summary>
    private static (Control Section, Scrollable Scrollable) BuildSection(Control list, Control details)
    {
        var scrollableList = new Scrollable
        {
            Content = list,
            Border = BorderType.None
        };

        var section = new StackLayout
        {
            Spacing = 16,

            Items =
            {
                new StackLayoutItem(scrollableList, true),
                details
            }
        };

        return (section, scrollableList);
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
        _materialDetails.ShowMaterial(result, _materialService.HasDefault(result));
    }

    private void DeactivateMaterial(Material material)
    {
        _materialService.SetActive(material.Id, false);

        _materialList.Refresh();
        _materialDetails.Clear();
    }

    /// <summary>
    /// Material is a reference type, so ResetToDefault() mutates the same
    /// instance the list and details panel already hold - refreshing and
    /// re-showing it is enough, no re-fetch needed. Only reachable while
    /// "Reset to Default" is enabled, which already requires HasDefault(), so
    /// this should always succeed - but if the material got deactivated/
    /// removed from under the user between showing it and clicking Reset,
    /// silently do nothing rather than show a stale/incorrect confirmation.
    /// </summary>
    private void ResetMaterialToDefault(Material material)
    {
        if (!_materialService.ResetToDefault(material.Id))
            return;

        _materialList.Refresh(material.Id);
        _materialDetails.ShowMaterial(material, hasDefault: true, statusMessage: "Reset to its default values.");
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
