using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class ReportService : IReportService
{
    private readonly KitchenDbContext _db;
    public ReportService(KitchenDbContext db) => _db = db;

    private IQueryable<Order> DeliveredInRange(DateTime from, DateTime to)
        => _db.Orders.Where(o => o.Status == "Delivered"
            && o.DeliveredAt >= from && o.DeliveredAt < to);

    public async Task<SalesSummaryDto> GetSummaryAsync(DateTime from, DateTime to)
    {
        var q = DeliveredInRange(from, to);
        var count = await q.CountAsync();
        var gross = await q.SumAsync(o => (decimal?)o.SubTotal) ?? 0;
        var discount = await q.SumAsync(o => (decimal?)o.Discount) ?? 0;
        var net = await q.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        return new SalesSummaryDto
        {
            From = from,
            To = to,
            OrderCount = count,
            GrossSales = gross,
            TotalDiscount = discount,
            NetSales = net,
            AverageOrderValue = count == 0 ? 0 : Math.Round(net / count, 2)
        };
    }

    public async Task<List<SalesByDayDto>> GetSalesByDayAsync(DateTime from, DateTime to)
    {
        return await DeliveredInRange(from, to)
            .GroupBy(o => o.DeliveredAt!.Value.Date)
            .Select(g => new SalesByDayDto
            {
                Date = g.Key,
                OrderCount = g.Count(),
                NetSales = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<List<SalesByItemDto>> GetSalesByItemAsync(DateTime from, DateTime to, int limit)
    {
        limit = Math.Clamp(limit, 1, 50);
        var orderIds = DeliveredInRange(from, to).Select(o => o.Id);

        return await _db.OrderItems
            .Where(i => orderIds.Contains(i.OrderId))
            .GroupBy(i => new { i.MenuItemId, i.ItemName })
            .Select(g => new SalesByItemDto
            {
                MenuItemId = g.Key.MenuItemId,
                ItemName = g.Key.ItemName,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<PeakHourDto>> GetPeakHoursAsync(DateTime from, DateTime to)
    {
        return await DeliveredInRange(from, to)
            .GroupBy(o => o.PlacedAt.Hour)
            .Select(g => new PeakHourDto { Hour = g.Key, OrderCount = g.Count() })
            .OrderBy(x => x.Hour)
            .ToListAsync();
    }
}