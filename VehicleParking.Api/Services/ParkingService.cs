using Microsoft.EntityFrameworkCore;
using VehicleParking.Api.Data;
using VehicleParking.Api.DTOs;
using VehicleParking.Api.Models;

namespace VehicleParking.Api.Services
{
    public class ParkingService : IParkingService
    {
        private readonly AppDbContext _context;

        public ParkingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParkingSlot>> GetAllSlotsAsync()
        {
            return await _context.ParkingSlots.ToListAsync();
        }

        public async Task<object> GetStatsAsync()
        {
            var carAvailable = await _context.ParkingSlots.CountAsync(s => s.VehicleType == VehicleType.Car && s.IsAvailable);
            var bikeAvailable = await _context.ParkingSlots.CountAsync(s => s.VehicleType == VehicleType.Bike && s.IsAvailable);
            var truckAvailable = await _context.ParkingSlots.CountAsync(s => s.VehicleType == VehicleType.Truck && s.IsAvailable);

            return new { carAvailable, bikeAvailable, truckAvailable };
        }

        public async Task<IEnumerable<ParkingSlot>> GetAvailableSlotsAsync()
        {
            return await _context.ParkingSlots.Where(s => s.IsAvailable).ToListAsync();
        }

        public async Task<object> BookSlotAsync(BookSlotDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var slot = await _context.ParkingSlots.FindAsync(dto.SlotId);
                if (slot == null || !slot.IsAvailable)
                {
                    throw new Exception("Slot is already booked or does not exist.");
                }

                slot.IsAvailable = false;
                slot.CurrentVehicleNumber = dto.VehicleNumber;

                string ticketNumber = "TKT-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                slot.TicketNumber = ticketNumber;

                decimal fee = slot.VehicleType switch
                {
                    VehicleType.Car => 100,
                    VehicleType.Bike => 50,
                    VehicleType.Truck => 200,
                    _ => 50
                };

                var parkingLog = new ParkingLog
                {
                    UserId = dto.UserId,
                    SlotId = dto.SlotId,
                    VehicleNumber = dto.VehicleNumber,
                    CheckInTime = DateTime.Now,
                    Status = "Active",
                    TicketNumber = ticketNumber,
                    Fee = fee
                };

                _context.ParkingLogs.Add(parkingLog);
                await _context.SaveChangesAsync();

                return new
                {
                    message = "Slot successfully booked!",
                    logId = parkingLog.Id,
                    ticketNumber = ticketNumber,
                    fee = fee
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<object> ReleaseSlotAsync(int slotId)
        {
            var slot = await _context.ParkingSlots.FindAsync(slotId);
            if (slot == null) throw new Exception("Slot not found.");
            if (slot.IsAvailable) throw new Exception("Slot is already available.");

            var activeLog = await _context.ParkingLogs.FirstOrDefaultAsync(l => l.SlotId == slotId && l.Status == "Active");

            decimal totalFee = 0;
            double totalHours = 1;

            decimal hourlyRate = slot.VehicleType switch
            {
                VehicleType.Car => 100,
                VehicleType.Bike => 50,
                VehicleType.Truck => 200,
                _ => 50
            };

            if (activeLog != null)
            {
                activeLog.CheckOutTime = DateTime.Now;
                activeLog.Status = "Completed";

                TimeSpan duration = activeLog.CheckOutTime.Value - activeLog.CheckInTime;
                totalHours = Math.Ceiling(duration.TotalHours);
                if (totalHours < 1) totalHours = 1;

                totalFee = (decimal)totalHours * hourlyRate;
                activeLog.Fee = totalFee;
            }
            else
            {
                totalFee = hourlyRate;
            }

            slot.IsAvailable = true;
            slot.CurrentVehicleNumber = null;
            slot.TicketNumber = null;

            await _context.SaveChangesAsync();

            return new
            {
                message = "Slot released successfully!",
                fee = totalFee,
                hours = totalHours
            };
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.ParkingLogs
                .Where(l => l.Status == "Completed")
                .SumAsync(l => (decimal?)l.Fee) ?? 0;
        }
    }
}