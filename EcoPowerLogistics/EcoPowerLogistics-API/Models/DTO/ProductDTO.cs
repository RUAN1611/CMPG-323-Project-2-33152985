namespace EcoPowerLogistics_API.Models.DTO
{
    public class ProductDTO
    {
        public short ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public int? UnitsInStock { get; set; }
    }
}
