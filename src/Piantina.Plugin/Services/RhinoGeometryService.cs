using Rhino;
using Rhino.Geometry;

namespace Piantina.Plugin.Services;

/// <summary>
/// Thin wrapper around RhinoCommon geometry queries needed by Piantina.Plugin
/// views. Kept out of Piantina.Core, which has no RhinoCommon dependency.
/// </summary>
public static class RhinoGeometryService
{
    /// <summary>
    /// Returns the combined volume, in cubic centimetres, of the currently selected
    /// objects in the active Rhino document, converted from whatever unit system the
    /// document is using (mm, cm, inches, etc). Returns null if there is no active
    /// document, nothing selected, or none of the selected objects is a closed
    /// (solid) Brep or mesh.
    ///
    /// Only closed/solid geometry is included. This matters for casting weight
    /// estimates specifically: VolumeMassProperties.Compute happily returns a
    /// number for an open surface or non-manifold mesh too, but that number isn't
    /// a real enclosed volume, so silently including it would produce a metal
    /// weight and cost that look plausible but are wrong. Excluding non-solid
    /// geometry means the caller sees "no solid object selected" instead of a
    /// bogus figure.
    /// </summary>
    public static decimal? GetSelectedVolumeInCubicCentimetres()
    {
        var doc = RhinoDoc.ActiveDoc;

        if (doc is null)
        {
            return null;
        }

        var selectedObjects = doc.Objects.GetSelectedObjects(false, false).ToList();

        if (selectedObjects.Count == 0)
        {
            return null;
        }

        var solidGeometries = selectedObjects
            .Select(rhinoObject => rhinoObject.Geometry)
            .Where(IsClosedSolid)
            .ToList();

        if (solidGeometries.Count == 0)
        {
            return null;
        }

        var volumeProperties = VolumeMassProperties.Compute(solidGeometries);

        if (volumeProperties is null)
        {
            return null;
        }

        var unitScaleToCentimetres = RhinoMath.UnitScale(doc.ModelUnitSystem, UnitSystem.Centimeters);
        var volumeScale = Math.Pow(unitScaleToCentimetres, 3);

        var volumeInCubicCentimetres = volumeProperties.Volume * volumeScale;

        return (decimal)volumeInCubicCentimetres;
    }

    /// <summary>
    /// One Piantina material's combined volume (cm³) across however many of
    /// the selected solid objects are wearing it.
    /// </summary>
    public record MaterialVolumeGroup(string MaterialName, decimal VolumeCm3);

    /// <summary>
    /// Splits the current selection's solid volume out by whichever Piantina
    /// material (see MaterialAppearanceService.TryParseMaterialName) each
    /// object is wearing - e.g. a two-tone ring with the shank materialed as
    /// one gold and the head as another via Apply to Selection. Groups are
    /// ordered largest volume first, on the assumption the bulk of a piece is
    /// usually its "main" metal. UnmatchedObjectCount counts solids that were
    /// skipped because they aren't wearing a Rhino material this plugin
    /// created - applied by hand, imported already-textured, or left at the
    /// layer default - so the caller can tell the user some geometry wasn't
    /// counted, rather than silently under-reporting the volume.
    /// </summary>
    public static (IReadOnlyList<MaterialVolumeGroup> Groups, int UnmatchedObjectCount) GetSelectedVolumesByMaterial()
    {
        var doc = RhinoDoc.ActiveDoc;

        if (doc is null)
        {
            return (Array.Empty<MaterialVolumeGroup>(), 0);
        }

        var selectedObjects = doc.Objects.GetSelectedObjects(false, false).ToList();

        var unitScaleToCentimetres = RhinoMath.UnitScale(doc.ModelUnitSystem, UnitSystem.Centimeters);
        var volumeScale = Math.Pow(unitScaleToCentimetres, 3);

        var volumeByMaterialName = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var unmatchedCount = 0;

        foreach (var rhinoObject in selectedObjects)
        {
            if (!IsClosedSolid(rhinoObject.Geometry))
            {
                continue;
            }

            var rhinoMaterial = rhinoObject.GetMaterial(true);
            var materialName = rhinoMaterial is null
                ? null
                : MaterialAppearanceService.TryParseMaterialName(rhinoMaterial.Name);

            if (materialName is null)
            {
                unmatchedCount++;
                continue;
            }

            var volumeProperties = VolumeMassProperties.Compute(rhinoObject.Geometry);

            if (volumeProperties is null)
            {
                unmatchedCount++;
                continue;
            }

            var volumeCm3 = (decimal)(volumeProperties.Volume * volumeScale);

            volumeByMaterialName[materialName] = volumeByMaterialName.GetValueOrDefault(materialName) + volumeCm3;
        }

        var groups = volumeByMaterialName
            .Select(pair => new MaterialVolumeGroup(pair.Key, pair.Value))
            .OrderByDescending(group => group.VolumeCm3)
            .ToList();

        return (groups, unmatchedCount);
    }

    /// <summary>
    /// True for geometry that fully encloses a volume: a closed (solid) Brep, a
    /// closed/manifold Mesh, or a closed Extrusion. Curves, points, open surfaces,
    /// annotations, and non-manifold meshes are excluded.
    /// </summary>
    private static bool IsClosedSolid(GeometryBase geometry)
    {
        return geometry switch
        {
            Brep brep => brep.IsSolid,
            Mesh mesh => mesh.IsClosed,
            Extrusion extrusion => extrusion.IsSolid,
            _ => false
        };
    }
}
