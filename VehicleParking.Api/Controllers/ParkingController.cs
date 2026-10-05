using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParking.Api.DTOs;
using VehicleParking.Api.Services;

namespace VehicleParking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ParkingController : ControllerBase
    {
        private readonly IParkingService _parkingService;

        // Dependency Injection (DIP Principle)
        public ParkingController(IParkingService parkingService)
        {
            _parkingService = parkingService;
        }

        [HttpGet("slots")]
        public async Task<IActionResult> GetAllSlots() => Ok(await _parkingService.GetAllSlotsAsync());

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats() => Ok(await _parkingService.GetStatsAsync());

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSlots() => Ok(await _parkingService.GetAvailableSlotsAsync());

        [HttpPost("book")]
        public async Task<IActionResult> BookSlot([FromBody] BookSlotDto dto)
        {
            try
            {
                var result = await _parkingService.BookSlotAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("release/{slotId}")]
        public async Task<IActionResult> ReleaseSlot(int slotId)
        {
            try
            {
                var result = await _parkingService.ReleaseSlotAsync(slotId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var revenue = await _parkingService.GetTotalRevenueAsync();
            return Ok(new { totalRevenue = revenue });
        }
    }
}