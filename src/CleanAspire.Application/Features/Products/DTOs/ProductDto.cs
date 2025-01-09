// This code defines a data transfer object (DTO) for products and an enumeration for product categories.
// The ProductDto class encapsulates product details for data transfer between application layers.
// The ProductCategoryDto enum provides predefined categories for products.

namespace CleanAspire.Application.Features.Products.DTOs; // Define the namespace for product-related DTOs.

// A DTO representing a product, used to transfer data between application layers.
public class ProductDto
{
    public string Id { get; set; } = string.Empty; // Unique identifier for the product.
    public string SKU { get; set; } = string.Empty; // Stock Keeping Unit, a unique code for tracking the product.
    public string Name { get; set; } = string.Empty; // The name of the product.
    public ProductCategoryDto? Category { get; set; } // Optional product category, using the ProductCategoryDto enum.
    public string? Description { get; set; } // Optional description of the product.
    public decimal Price { get; set; } // The price of the product.
    public string? Currency { get; set; } // Optional currency in which the price is listed (e.g., USD, EUR).
    public string? UOM { get; set; } // Optional Unit of Measurement for the product (e.g., kg, pcs).
}

// An enumeration representing possible product categories.
public enum ProductCategoryDto
{
    Electronics, // Products like mobile phones, laptops, etc.
    Furniture,   // Products like tables, chairs, and sofas.
    Clothing,    // Apparel like shirts, pants, and jackets.
    Food,        // Edible products like fruits, vegetables, and snacks.
    Beverages,   // Drinks like coffee, tea, and juice.
    HealthCare,  // Products related to health and wellness.
    Sports       // Sports-related items like equipment and apparel.
}

