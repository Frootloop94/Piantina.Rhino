using System.Drawing;
using Piantina.Core.Materials;
using Rhino;

namespace Piantina.Plugin.Services;

/// <summary>
/// Applies a Material's appearance (colour, reflectivity, shine) plus a chosen
/// surface finish to the currently selected Rhino objects as a real Rhino
/// render material - visible immediately in Rendered/Arctic viewport display
/// modes without needing a raytraced render, the same "quick preview" role
/// RhinoGold's material palette used to serve.
/// </summary>
public static class MaterialAppearanceService
{
    public enum ApplyResult
    {
        Applied,
        NoDocument,
        NothingSelected
    }

    public static ApplyResult ApplyToSelection(Material material, MetalFinish finish)
    {
        var doc = RhinoDoc.ActiveDoc;

        if (doc is null)
        {
            return ApplyResult.NoDocument;
        }

        var selectedObjects = doc.Objects.GetSelectedObjects(false, false).ToList();

        if (selectedObjects.Count == 0)
        {
            return ApplyResult.NothingSelected;
        }

        var materialIndex = GetOrCreateRhinoMaterial(doc, material, finish);

        foreach (var rhinoObject in selectedObjects)
        {
            rhinoObject.Attributes.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
            rhinoObject.Attributes.MaterialIndex = materialIndex;
            rhinoObject.CommitChanges();
        }

        doc.Views.Redraw();

        return ApplyResult.Applied;
    }

    /// <summary>
    /// Finds the Rhino material previously created for this Material+finish
    /// combination (matched by name) so repeated clicks don't pile up duplicate
    /// materials in the document - only the first click on a given swatch and
    /// finish actually creates one.
    /// </summary>
    private static int GetOrCreateRhinoMaterial(RhinoDoc doc, Material material, MetalFinish finish)
    {
        var materialName = BuildMaterialName(material, finish);

        var existing = doc.Materials.FirstOrDefault(m => m.Name == materialName);

        if (existing is not null)
        {
            return existing.Index;
        }

        var rhinoMaterial = new Rhino.DocObjects.Material
        {
            Name = materialName,
            DiffuseColor = Color.FromArgb(material.ColorR, material.ColorG, material.ColorB),
            SpecularColor = Color.FromArgb(255, 255, 255),
            Reflectivity = material.Reflectivity,
            Shine = material.Shine * ShineMultiplier(finish) * Rhino.DocObjects.Material.MaxShine
        };

        return doc.Materials.Add(rhinoMaterial);
    }

    private static string BuildMaterialName(Material material, MetalFinish finish) =>
        $"Piantina - {material.Name} ({finish})";

    /// <summary>
    /// Hammered and sand-blasted surfaces scatter light rather than reflecting it
    /// sharply, so they get a duller highlight than the material's own Shine
    /// value would give on its own.
    /// </summary>
    private static double ShineMultiplier(MetalFinish finish) => finish switch
    {
        MetalFinish.Polished => 1.0,
        MetalFinish.Hammered => 0.45,
        MetalFinish.SandBlast => 0.15,
        _ => 1.0
    };
}
