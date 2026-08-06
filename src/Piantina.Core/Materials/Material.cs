namespace Piantina.Core.Materials;

public class Material
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public MaterialCategory Category { get; set; }

    public decimal Density { get; set; }

    public decimal PricePerGram { get; set; }

    public bool IsActive { get; set; } = true;

    public string Notes { get; set; } = string.Empty;
}
