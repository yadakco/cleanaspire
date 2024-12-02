namespace CleanAspire.Application.Features.Products.DTOs;
public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ProductCategoryDto? Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public string? UOM { get; set; }
}


public enum ProductCategoryDto
{
    Electronics,
    Furniture,
    Clothing,
    Food,
    Beverages,
    HealthCare,
    Sports,
}

