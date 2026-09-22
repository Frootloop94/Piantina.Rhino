namespace Piantina.Core.Materials;

public class MaterialService
{
    private readonly MaterialRepository _repository;
    private readonly List<Material> _materials;

    public MaterialService(MaterialRepository? repository = null)
    {
        _repository = repository ?? new MaterialRepository();

        _materials = _repository.Load();

        if (_materials.Count == 0)
        {
            _materials.AddRange(GetDefaultMaterials());
            Persist();
        }
    }

    /// <summary>
    /// This catalog mirrors the actual metal/master alloy list the business
    /// offers. Density and appearance (colour/reflectivity/shine) are typical
    /// values for each alloy type, not a certified spec sheet for a specific
    /// supplier - close enough for weight estimates and viewport swatches, but
    /// worth checking against the supplier's own figures for anything
    /// cost-critical. PricePerGram is deliberately left at 0 here: that's
    /// current market/supplier pricing, which has to come from the business,
    /// not a guess baked into the plugin.
    /// </summary>
    private static IEnumerable<Material> GetDefaultMaterials()
    {
        // This order is the business's own metal list, not alphabetical -
        // fine gold, then each karat from 22ct down to 9ct (white, then
        // yellow, then rose within a karat), then the four silvers. SortOrder
        // is assigned below from this list's position, so the list itself is
        // the single source of truth for both content and display order.
        var materials = new List<Material>
        {
            NewMaterial("24ct Fine Gold", MaterialCategory.Gold, 19.32m, 230, 186, 63, 0.75, 0.80),

            NewMaterial("22ct Standard White Gold", MaterialCategory.Gold, 17.70m, 224, 210, 180, 0.73, 0.80),
            NewMaterial("22ct Standard Yellow Gold", MaterialCategory.Gold, 17.80m, 224, 180, 72, 0.72, 0.79),
            NewMaterial("22ct Standard Rose Gold", MaterialCategory.Gold, 17.50m, 225, 169, 118, 0.70, 0.78),

            NewMaterial("18ct White Gold 10,00%PD", MaterialCategory.Gold, 15.90m, 213, 207, 198, 0.74, 0.82),
            NewMaterial("18ct White Gold 00,00%PD", MaterialCategory.Gold, 14.70m, 208, 205, 199, 0.72, 0.80),
            NewMaterial("18ct Standard Yellow Gold", MaterialCategory.Gold, 15.60m, 222, 184, 97, 0.70, 0.78),
            NewMaterial("18ct Standard Rose Gold", MaterialCategory.Gold, 15.20m, 219, 158, 128, 0.68, 0.76),

            NewMaterial("14ct White Gold 10,00%PD", MaterialCategory.Gold, 13.70m, 206, 201, 194, 0.71, 0.80),
            NewMaterial("14ct White Gold 00,00%PD", MaterialCategory.Gold, 12.70m, 203, 200, 196, 0.69, 0.78),
            NewMaterial("14ct Standard Yellow Gold", MaterialCategory.Gold, 13.07m, 211, 175, 97, 0.66, 0.75),
            NewMaterial("14ct Standard Rose Gold", MaterialCategory.Gold, 13.00m, 207, 145, 118, 0.65, 0.74),

            NewMaterial("9ct White Gold 10,00%PD", MaterialCategory.Gold, 11.50m, 199, 195, 189, 0.68, 0.77),
            NewMaterial("9ct White Gold 00,00%PD", MaterialCategory.Gold, 10.70m, 197, 194, 190, 0.66, 0.76),
            NewMaterial("9ct Standard Yellow Gold", MaterialCategory.Gold, 11.00m, 199, 165, 100, 0.62, 0.72),
            NewMaterial("9ct Standard Rose Gold", MaterialCategory.Gold, 11.10m, 195, 139, 113, 0.61, 0.71),

            NewMaterial("Fine Silver", MaterialCategory.Silver, 10.49m, 226, 226, 230, 0.82, 0.88),
            NewMaterial("Sterling Silver", MaterialCategory.Silver, 10.36m, 215, 215, 218, 0.80, 0.85),
            NewMaterial("Tarnish Resistant Silver", MaterialCategory.Silver, 10.40m, 218, 219, 222, 0.80, 0.86),
            NewMaterial("Platinum Silver", MaterialCategory.Silver, 10.45m, 220, 221, 224, 0.81, 0.87)
        };

        for (var i = 0; i < materials.Count; i++)
        {
            materials[i].SortOrder = i;
        }

        return materials;
    }

    private static Material NewMaterial(
        string name,
        MaterialCategory category,
        decimal density,
        byte colorR,
        byte colorG,
        byte colorB,
        double reflectivity,
        double shine) => new()
    {
        Name = name,
        Category = category,
        Density = density,
        PricePerGram = 0m,
        ColorR = colorR,
        ColorG = colorG,
        ColorB = colorB,
        Reflectivity = reflectivity,
        Shine = shine
    };

    /// <summary>
    /// Adds any built-in default material not already present (matched by
    /// name) without touching existing materials - lets an install that
    /// already has a persisted catalog pick up newly added/changed defaults
    /// (like this list being extended from 4 materials to the business's
    /// full real metal list) without wiping anything already added or edited.
    /// Returns how many were added.
    /// </summary>
    public int AddMissingDefaults()
    {
        var existingNames = _materials
            .Select(material => material.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = GetDefaultMaterials()
            .Where(material => !existingNames.Contains(material.Name))
            .ToList();

        if (missing.Count == 0)
        {
            return 0;
        }

        _materials.AddRange(missing);
        Persist();

        return missing.Count;
    }

    /// <summary>
    /// Names seeded by an older version of the default catalog that no
    /// longer match anything in GetDefaultMaterials() - "18ct Yellow" and
    /// "950 Platinum" were the generic 4-material placeholder set, replaced
    /// by the business's real metal list (e.g. "18ct Standard Yellow Gold").
    /// </summary>
    private static readonly string[] LegacyDefaultNames =
    {
        "18ct Yellow",
        "950 Platinum"
    };

    /// <summary>
    /// Deactivates any still-active material matching a known superseded
    /// default name (see LegacyDefaultNames), without touching anything the
    /// user added or renamed themselves. Deactivating rather than deleting
    /// keeps any historical costing that referenced them intact. Returns how
    /// many were deactivated.
    /// </summary>
    public int DeactivateLegacyDefaults()
    {
        var legacyNames = LegacyDefaultNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toDeactivate = _materials
            .Where(material => material.IsActive && legacyNames.Contains(material.Name))
            .ToList();

        if (toDeactivate.Count == 0)
        {
            return 0;
        }

        foreach (var material in toDeactivate)
        {
            material.IsActive = false;
        }

        Persist();

        return toDeactivate.Count;
    }

    public IReadOnlyList<Material> GetMaterials(bool includeInactive = false)
    {
        return _materials
            .Where(material => includeInactive || material.IsActive)
            .ToList();
    }

    public IEnumerable<IGrouping<MaterialCategory, Material>> GetMaterialsByCategory(bool includeInactive = false)
    {
        return _materials
            .Where(material => includeInactive || material.IsActive)
            .GroupBy(material => material.Category);
    }

    public void AddMaterial(Material material)
    {
        _materials.Add(material);
        Persist();
    }

    public void SetActive(Guid materialId, bool isActive)
    {
        var material = _materials.FirstOrDefault(m => m.Id == materialId);

        if (material is not null)
        {
            material.IsActive = isActive;
            Persist();
        }
    }

    /// <summary>
    /// Call after an in-place edit (e.g. via MaterialEditorDialog, which mutates the
    /// existing Material instance directly) so the change is saved to disk. There's
    /// no separate UpdateMaterial method because there's nothing to "update" in the
    /// in-memory list - the same object reference is already there.
    /// </summary>
    public void NotifyMaterialUpdated()
    {
        Persist();
    }

    private void Persist()
    {
        _repository.Save(_materials);
    }
}
