

using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities
{
    public class Product: BaseAuditableEntity,IAuditTrial
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Currency { get; set; }
        public int Quantity { get; set; }
        public string? UOM { get; set; } 
        public byte[]? Image { get; set; }
    }
}
