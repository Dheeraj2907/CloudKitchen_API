namespace CloudKitchen_API.Services.Dto
{
    public class MenuItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal HalfPrice { get; set; }

        public string? ImageUrl { get; set; }

        public bool? IsAvailable { get; set; }

        public decimal FullPrice { get; set; }

    }
}
