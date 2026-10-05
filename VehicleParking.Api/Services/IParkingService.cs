using Microsoft.AspNetCore.Mvc;
using VehicleParking.Api.DTOs;
using VehicleParking.Api.Models;

namespace VehicleParking.Api.Services
{
    public interface IParkingService
    {
        Task<IEnumerable<ParkingSlot>> GetAllSlotsAsync();
        Task<object> GetStatsAsync();
        Task<IEnumerable<ParkingSlot>> GetAvailableSlotsAsync();
        Task<object> BookSlotAsync(BookSlotDto dto);
        Task<object> ReleaseSlotAsync(int slotId);
        Task<decimal> GetTotalRevenueAsync();
    }
}