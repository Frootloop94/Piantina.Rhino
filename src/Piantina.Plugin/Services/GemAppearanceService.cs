using System.Drawing;
using Piantina.Core.Gems;
using Rhino;

namespace Piantina.Plugin.Services;

/// <summary>
/// Applies a Gemstone's appearance (colour, transparency, refractive index)
/// to the currently selected Rhino objects as a real Rhino render material -
/// visible immediately in Rendered/Arctic viewport display modes without
/// needing a raytraced render. Separate from MaterialAppearanceService since
/// a gem needs a transparent/refractive material rather than an opaque
/// metallic one, and has no Polish/Hammered/Sand Blast finish concept.
/// </summary>
public static class GemAppearanceService
{
    public enum ApplyResult
    {
        Applied,
        NoDocument,
        NothingSelected
    }

    public static ApplyResult ApplyToSelection(Gemstone gemstone)
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

        var materialIndex = GetOrCreateRhinoMaterial(doc, gemstone);

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
    /// Finds the Rhino material previously created for this gem (matched by
    /// name) so repeated applies don't pile up duplicate materials in the
    /// document - only the first apply of a given gem creates one.
    /// </summary>
    private static int GetOrCreateRhinoMaterial(RhinoDoc doc, Gemstone gemstone)
    {
        var materialName = $"Piantina - {gemstone.Name} (Gem)";

        var existing = doc.Materials.FirstOrDefault(m => m.Name == materialName);

        if (existing is not null)
        {
            return existing.Index;
        }

        var rhinoMaterial = new Rhino.DocObjects.Material
        {
            Name = materialName,
            DiffuseColor = Color.FromArgb(gemstone.ColorR, gemstone.ColorG, gemstone.ColorB),
            SpecularColor = Color.FromArgb(255, 255, 255),
            Transparency = gemstone.Transparency,
            IndexOfRefraction = gemstone.RefractiveIndex,
            Reflectivity = gemstone.Reflectivity,
            Shine = gemstone.Shine * Rhino.DocObjects.Material.MaxShine
        };

        return doc.Materials.Add(rhinoMaterial);
    }
}
