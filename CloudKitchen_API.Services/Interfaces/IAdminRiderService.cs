using CloudKitchen_API.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Interfaces
{
    public interface IAdminRiderService
    {
        Task<List<RiderAdminDto>> GetAllAsync();
        Task<RiderAdminDto> CreateAsync(RiderCreateRequest request);
        Task<RiderAdminDto> UpdateAsync(Guid id, RiderCreateRequest request);
        Task SetAvailabilityAsync(Guid id, bool available);
        Task DeleteAsync(Guid id);
    }
}
