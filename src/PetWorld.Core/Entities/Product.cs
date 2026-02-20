namespace PetWorld.Core.Entities;

/// <summary>
/// Product catalog entity.
/// This is the "source of truth" that prevents AI hallucinations.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}