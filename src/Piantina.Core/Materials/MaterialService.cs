namespace Piantina.Core.Materials;

public class MaterialService
{
    public IReadOnlyList<Material> GetMaterials()
    {
        return new List<Material>
        {
            new()
            {
                Name = "24ct Fine Gold",
                Category = MaterialCategory.Gold,
                Density = 19.32m,
                PricePerGram = 1532.40m
            },

            new()
            {
                Name = "18ct Yellow",
                Category = MaterialCategory.Gold,
                Density = 15.60m,
                PricePerGram = 1148.20m
            },

            new()
            {
                Name = "Sterling Silver",
                Category = MaterialCategory.Silver,
                Density = 10.36m,
                PricePerGram = 18.40m
            },

            new()
            {
                Name = "950 Platinum",
                Category = MaterialCategory.Platinum,
                Density = 21.45m,
                PricePerGram = 648.10m
            }
        };
    }

    public IEnumerable<IGrouping<MaterialCategory, Material>>
    GetMaterialsByCategory()
    {
        return GetMaterials()
            .GroupBy(m => m.Category);
    }
}
