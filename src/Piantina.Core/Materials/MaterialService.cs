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

    private static IEnumerable<Material> GetDefaultMaterials()
    {
        yield return new Material
        {
            Name = "24ct Fine Gold",
            Category = MaterialCategory.Gold,
            Density = 19.32m,
            PricePerGram = 1532.40m
        };

        yield return new Material
        {
            Name = "18ct Yellow",
            Category = MaterialCategory.Gold,
            Density = 15.60m,
            PricePerGram = 1148.20m
        };

        yield return new Material
        {
            Name = "Sterling Silver",
            Category = MaterialCategory.Silver,
            Density = 10.36m,
            PricePerGram = 18.40m
        };

        yield return new Material
        {
            Name = "950 Platinum",
            Category = MaterialCategory.Platinum,
            Density = 21.45m,
            PricePerGram = 648.10m
        };
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
