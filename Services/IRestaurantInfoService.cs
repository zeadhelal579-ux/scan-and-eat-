// FILE: Services/IRestaurantInfoService.cs
// PURPOSE: Contract for the Home-page content resource, used by
//          RestaurantInfoController.

using ScanEat.Api.Models.DTOs;

namespace ScanEat.Api.Services;

public interface IRestaurantInfoService
{
    Task<RestaurantInfoDto> GetAsync();
}
