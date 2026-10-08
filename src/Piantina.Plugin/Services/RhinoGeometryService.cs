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
