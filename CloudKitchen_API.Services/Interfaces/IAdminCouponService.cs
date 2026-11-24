using CloudKitchen_API.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Interfaces
{
    public interface IAdminCouponService
    {
        Task<List<CouponListDto>> GetAllAsync(bool includeInactive);
        Task<CouponListDto> CreateAsync(AdminCouponRequest request);
        Task<CouponListDto> UpdateAsync(Guid id, AdminCouponRequest request);
        Task SetActiveAsync(Guid id, bool active);
        Task DeleteAsync(Guid id);
    }
}
