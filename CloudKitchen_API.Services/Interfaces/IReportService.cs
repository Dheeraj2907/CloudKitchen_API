using CloudKitchen_API.Services.Dto;

namespace CloudKitchen_API.Services.Interfaces;

public interface IReportService
{
    Task<SalesSummaryDto> GetSummaryAsync(DateTime from, DateTime to);
    Task<List<SalesByDayDto>> GetSalesByDayAsync(DateTime from, DateTime to);
    Task<List<SalesByItemDto>> GetSalesByItemAsync(DateTime from, DateTime to, int limit);
    Task<List<PeakHourDto>> GetPeakHoursAsync(DateTime from, DateTime to);
}