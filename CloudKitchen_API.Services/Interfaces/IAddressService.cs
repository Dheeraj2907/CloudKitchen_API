using CloudKitchen_API.Services.Dto;
namespace CloudKitchen_API.Services.Interfaces;

public interface IAddressService
{
    Task<List<AddressDto>> GetAsync(Guid userId);
    Task<AddressDto> AddAsync(Guid userId, AddressCreateRequest request);
    Task<AddressDto> UpdateAsync(Guid userId, Guid addressId, AddressCreateRequest request);
    Task DeleteAsync(Guid userId, Guid addressId);
    Task SetDefaultAsync(Guid userId, Guid addressId);
}