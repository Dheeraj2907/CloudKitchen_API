namespace CloudKitchen_API.Services.Dto;

public class SalesSummaryDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int OrderCount { get; set; }
    public decimal GrossSales { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetSales { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class SalesByDayDto
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal NetSales { get; set; }
}

public class SalesByItemDto
{
    public Guid MenuItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class PeakHourDto
{
    public int Hour { get; set; }
    public int OrderCount { get; set; }
}