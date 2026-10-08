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
    /// offers, plus a handful of non-metal viewing aids (see the Plastic/Other
    /// entries at the end). Density and appearance (colour/reflectivity/shine)
    /// are typical values for each alloy type, not a certified spec sheet for
    /// a specific supplier - close enough for weight estimates and viewport
    /// swatches, but worth checking against the supplier's own figures for
    /// anything cost-critical. PricePerGram is deliberately left at 0 here:
    /// that's current market/supplier pricing, which has to come from the
    /// business, not a guess baked into the plugin.
    /// </summary>
    private static IEnumerable<Material> GetDefaultMaterials()
    {
        // This order is the business's own metal list, not alphabetical -
        // fine gold, then each karat from 22ct down to 9ct (white, then
        // yellow, then rose within a karat), then the four silvers, then the
        // viewing-only extras. SortOrder is assigned below from this list's
        // position, so the list itself is the single source of truth for both
        // content and display order.
        var materials = new List<Material>
        {
            NewMaterial("24ct Fine Gold", MaterialCategory.Gold, 19.32m, 230, 175, 21, 0.75, 0.80),

            NewMaterial("22ct Standard White Gold", MaterialCategory.Gold, 17.70m, 224, 210, 180, 0.73, 0.80),
            NewMaterial("22ct Standard Yellow Gold", MaterialCategory.Gold, 17.80m, 224, 169, 34, 0.72, 0.79),
            NewMaterial("22ct Standard Rose Gold", MaterialCategory.Gold, 17.50m, 225, 155, 91, 0.70, 0.78),

            NewMaterial("18ct White Gold 10,00%PD", MaterialCategory.Gold, 15.90m, 213, 207, 198, 0.74, 0.82),
            NewMaterial("18ct White Gold 00,00%PD", MaterialCategory.Gold, 14.70m, 208, 205, 199, 0.72, 0.80),
            NewMaterial("18ct Standard Yellow Gold", MaterialCategory.Gold, 15.60m, 222, 174, 66, 0.70, 0.78),
            NewMaterial("18ct Standard Rose Gold", MaterialCategory.Gold, 15.20m, 219, 143, 105, 0.68, 0.76),

            NewMaterial("14ct White Gold 10,00%PD", MaterialCategory.Gold, 13.70m, 206, 201, 194, 0.71, 0.80),
            NewMaterial("14ct White Gold 00,00%PD", MaterialCategory.Gold, 12.70m, 203, 200, 196, 0.69, 0.78),
            NewMaterial("14ct Standard Yellow Gold", MaterialCategory.Gold, 13.07m, 211, 166, 68, 0.66, 0.75),
            NewMaterial("14ct Standard Rose Gold", MaterialCategory.Gold, 13.00m, 207, 129, 96, 0.65, 0.74),

            NewMaterial("9ct White Gold 10,00%PD", MaterialCategory.Gold, 11.50m, 199, 195, 189, 0.68, 0.77),
            NewMaterial("9ct White Gold 00,00%PD", MaterialCategory.Gold, 10.70m, 197, 194, 190, 0.66, 0.76),
            NewMaterial("9ct Standard Yellow Gold", MaterialCategory.Gold, 11.00m, 199, 156, 75, 0.62, 0.72),
            NewMaterial("9ct Standard Rose Gold", MaterialCategory.Gold, 11.10m, 195, 125, 92, 0.61, 0.71),

            NewMaterial("Fine Silver", MaterialCategory.Silver, 10.49m, 226, 226, 230, 0.82, 0.88),
            NewMaterial("Sterling Silver", MaterialCategory.Silver, 10.36m, 215, 215, 218, 0.80, 0.85),
            NewMaterial("Tarnish Resistant Silver", MaterialCategory.Silver, 10.40m, 218, 219, 222, 0.80, 0.86),
            NewMaterial("Platinum Silver", MaterialCategory.Silver, 10.45m, 220, 221, 224, 0.81, 0.87),

            // Viewing-only prototype materials: for previewing a design's
            // form in a rough, unpolished stand-in colour before a metal
            // choice/costing is locked in, not something the business stocks
            // or prices - hence the matte reflectivity/shine (plastic, not
            // polished metal) and a generic ABS-like density shared by all
            // four colourways. Black Rhodium is the one genuine finish here -
            // a plated-on precious-metal coating rather than a bulk metal in
            // its own right, so it doesn't fit any of the metal categories
            // above; its density is rhodium's own, and its higher
            // reflectivity/shine reflect its plated, semi-gloss look next to
            // the matte plastics.
            NewMaterial("Grey Plastic", MaterialCategory.Plastic, 1.05m, 150, 150, 150, 0.15, 0.20),
            NewMaterial("Red Plastic", MaterialCategory.Plastic, 1.05m, 200, 90, 75, 0.15, 0.20),
            NewMaterial("Yellow Plastic", MaterialCategory.Plastic, 1.05m, 230, 185, 40, 0.15, 0.20),
            NewMaterial("Black Plastic", MaterialCategory.Plastic, 1.05m, 30, 30, 30, 0.08, 0.12),
            NewMaterial("Black Rhodium", MaterialCategory.Other, 12.41m, 35, 35, 40, 0.55, 0.65)
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

    /// <summary>
    /// The appearance a gold default carried under a previous colour revision,
    /// where the yellow/rose golds came out too washed out/desaturated. Keyed by
    /// material name so RepairMissingAppearance() can recognise a material
    /// still sitting at that superseded colour and refresh it to the current
    /// default, the same way it already recognises the plain-grey default.
    /// </summary>
    private static readonly Dictionary<string, (byte R, byte G, byte B)> SupersededAppearanceByName =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["24ct Fine Gold"] = (230, 186, 63),
            ["22ct Standard Yellow Gold"] = (224, 180, 72),
            ["22ct Standard Rose Gold"] = (225, 169, 118),
            ["18ct Standard Yellow Gold"] = (222, 184, 97),
            ["18ct Standard Rose Gold"] = (219, 158, 128),
            ["14ct Standard Yellow Gold"] = (211, 175, 97),
            ["14ct Standard Rose Gold"] = (207, 145, 118),
            ["9ct Standard Yellow Gold"] = (199, 165, 100),
            ["9ct Standard Rose Gold"] = (195, 139, 113)
        };

    /// <summary>
    /// Refreshes the appearance (colour/reflectivity/shine) of any material
    /// still sitting at Material's own class default (plain grey, 200/200/200)
    /// or at a superseded gold colour (see SupersededAppearanceByName) from
    /// the current default catalog's entry of the same name - covers a
    /// material that existed before appearance fields did (like "24ct Fine
    /// Gold" showing up grey/white instead of gold) that AddMissingDefaults()
    /// skips because it already exists by name. Only touches materials still
    /// at one of those exact colours, since a real custom colour landing on
    /// precisely one of them is extremely unlikely, so this shouldn't clobber
    /// an intentional edit. Doesn't touch density, price or anything else.
    /// Returns how many were repaired.
    /// </summary>
    public int RepairMissingAppearance()
    {
        const byte defaultColor = 200;

        var defaultsByName = GetDefaultMaterials()
            .ToDictionary(material => material.Name, StringComparer.OrdinalIgnoreCase);

        var repaired = 0;

        foreach (var material in _materials)
        {
            var isPlainDefault = material.ColorR == defaultColor
                && material.ColorG == defaultColor
                && material.ColorB == defaultColor;

            var isSupersededGold = SupersededAppearanceByName.TryGetValue(material.Name, out var superseded)
                && material.ColorR == superseded.R
                && material.ColorG == superseded.G
                && material.ColorB == superseded.B;

            if (!isPlainDefault && !isSupersededGold)
            {
                continue;
            }

            if (!defaultsByName.TryGetValue(material.Name, out var defaultMaterial))
            {
                continue;
            }

            material.ColorR = defaultMaterial.ColorR;
            material.ColorG = defaultMaterial.ColorG;
            material.ColorB = defaultMaterial.ColorB;
            material.Reflectivity = defaultMaterial.Reflectivity;
            material.Shine = defaultMaterial.Shine;

            repaired++;
        }

        if (repaired > 0)
        {
            Persist();
        }

        return repaired;
    }

    /// <summary>
    /// True if this material's name matches a built-in default, meaning
    /// ResetToDefault() has something to reset it back to. False for anything
    /// the user added themselves, or a built-in they've since renamed - used
    /// to grey out the "Reset to Default" button rather than let it silently
    /// do nothing.
    /// </summary>
    public bool HasDefault(Material material) =>
        GetDefaultMaterials().Any(d => string.Equals(d.Name, material.Name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Resets a single material's catalog-defined fields - category, density,
    /// and appearance (colour/reflectivity/shine) - back to the current
    /// built-in default for its name, undoing an accidental or exploratory
    /// edit in MaterialEditorDialog. Deliberately leaves PricePerGram (the
    /// business's own pricing, never set by a default - see
    /// GetDefaultMaterials) and Notes (the user's own annotation) untouched,
    /// and leaves Name/IsActive/SortOrder alone too. No-op, returning false,
    /// if the material isn't found or its name doesn't match a built-in
    /// default.
    /// </summary>
    public bool ResetToDefault(Guid materialId)
    {
        var material = _materials.FirstOrDefault(m => m.Id == materialId);

        if (material is null)
        {
            return false;
        }

        var defaultMaterial = GetDefaultMaterials()
            .FirstOrDefault(d => string.Equals(d.Name, material.Name, StringComparison.OrdinalIgnoreCase));

        if (defaultMaterial is null)
        {
            return false;
        }

        material.Category = defaultMaterial.Category;
        material.Density = defaultMaterial.Density;
        material.ColorR = defaultMaterial.ColorR;
        material.ColorG = defaultMaterial.ColorG;
        material.ColorB = defaultMaterial.ColorB;
        material.Reflectivity = defaultMaterial.Reflectivity;
        material.Shine = defaultMaterial.Shine;

        Persist();

        return true;
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
