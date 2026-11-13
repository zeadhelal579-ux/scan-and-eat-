// FILE: Services/IOfferService.cs
// PURPOSE: Contract for Offer business logic, used by OffersController.

using ScanEat.Api.Models.DTOs;

namespace ScanEat.Api.Services;

public interface IOfferService
{
    Task<List<OfferDto>> GetAllAsync();
    Task<OfferDto> CreateAsync(OfferUpsertDto dto);
    Task<OfferDto> UpdateAsync(int id, OfferUpsertDto dto);
    Task DeleteAsync(int id);
}
