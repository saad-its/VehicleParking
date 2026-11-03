using Microsoft.EntityFrameworkCore;
using VehicleParking.Api.Data;
using VehicleParking.Api.Models;
using VehicleParking.Api.Services;
using NUnit.Framework.Internal;

namespace ParkingSystem.Tests
{
    [TestFixture]
    public class ParkingServiceTests
    {
        private AppDbContext _context;
        private IParkingService _parkingService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "ParkingTestDb_" + System.Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            _context = new AppDbContext(options);
            _parkingService = new ParkingService(_context);
        }
        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        [Test]
        public async Task GetAllSlotsAsync_ShouldReturnAllSlots_WhenSlotsExistInDatabase()
        {
            var slot1 = new ParkingSlot { SlotNumber = "A1", IsAvailable = true, VehicleType = VehicleType.Car };
            var slot2 = new ParkingSlot { SlotNumber = "A2", IsAvailable = false, VehicleType = VehicleType.Bike };

            await _context.ParkingSlots.AddRangeAsync(slot1, slot2);
            await _context.SaveChangesAsync();

            var result = await _parkingService.GetAllSlotsAsync();

            Assert.That(result, Is.Not.Null);

            var slotList = result.ToList();
            Assert.That(slotList.Count, Is.EqualTo(2));
            Assert.That(slotList[0].SlotNumber, Is.EqualTo("A1"));
        }

        [Test]
        public async Task GetAvailableSlotsAsync_ShouldReturnOnlyAvilableSlots()
        {
            var slot1 = new ParkingSlot { SlotNumber = "A1", IsAvailable = true, VehicleType = VehicleType.Car };
            var slot2 = new ParkingSlot { SlotNumber = "A2", IsAvailable = false, VehicleType = VehicleType.Bike };

            await _context.ParkingSlots.AddRangeAsync(slot1, slot2);
            await _context.SaveChangesAsync();

            var result = await _parkingService.GetAvailableSlotsAsync();

            Assert.That(result, Is.Not.Null);

            var availableList = result.ToList();
            Assert.That(availableList[0].SlotNumber, Is.EqualTo("A1"));
        }

        [Test]
        public async Task GetStatsAsync_ShouldReturnCorrectCount()
        {
            var slot1 = new ParkingSlot { SlotNumber = "A1", IsAvailable = true, VehicleType = VehicleType.Car };
            var slot2 = new ParkingSlot { SlotNumber = "B1", IsAvailable = false, VehicleType = VehicleType.Bike };
            var slot3 = new ParkingSlot { SlotNumber = "C1", IsAvailable = true, VehicleType = VehicleType.Truck };

            await _context.ParkingSlots.AddRangeAsync(slot1, slot2, slot3);
            await _context.SaveChangesAsync();

            var result = await _parkingService.GetStatsAsync();

            Assert.That(result, Is.Not.Null);

            var resultType = result.GetType();
            int carCount = (int)resultType.GetProperty("carAvailable")!.GetValue(result)!;
            int bikeCount = (int)resultType.GetProperty("bikeAvailable")!.GetValue(result)!;
            int truckCount = (int)resultType.GetProperty("truckAvailable")!.GetValue(result)!;

            Assert.That(carCount, Is.EqualTo(1));
            Assert.That(bikeCount, Is.EqualTo(0));
            Assert.That(truckCount, Is.EqualTo(1));

        }

        [Test]
        public async Task BookSlotAsync_ShouldBookSlotAndCreateLog()
        {
            var slot = new ParkingSlot
            {
                SlotNumber = "A1",
                IsAvailable = true,
                VehicleType = VehicleType.Car
            };
            await _context.ParkingSlots.AddAsync(slot);
            await _context.SaveChangesAsync();

            var dto = new VehicleParking.Api.DTOs.BookSlotDto
            {
                SlotId = slot.Id,
                VehicleNumber = "ABC-123",
            };

            await _parkingService.BookSlotAsync(dto);

            var updatedSlot = await _context.ParkingSlots.FindAsync(slot.Id);
            Assert.That(updatedSlot, Is.Not.Null);
            Assert.That(updatedSlot.IsAvailable, Is.EqualTo(false));

            var logCount = _context.ParkingLogs.Count();
            Assert.That(logCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ReleaseSlotAsync_ShouldFreeSlotAndCompleteLog()
        {
            var slot = new ParkingSlot
            {
                SlotNumber = "A1",
                IsAvailable = false,
                VehicleType = VehicleType.Car,
            };
            await _context.ParkingSlots.AddAsync(slot);
            await _context.SaveChangesAsync();

            await _parkingService.ReleaseSlotAsync(slot.Id);

            var updatedSlot = await _context.ParkingSlots.FindAsync(slot.Id);
            Assert.That(updatedSlot, Is.Not.Null);
            Assert.That(updatedSlot.IsAvailable, Is.EqualTo(true));
        }
        
        [Test]
        public async Task GetTotalRevenueAsync_ShouldReturnCorrectTotalAmount()
        {
            var log1 = new ParkingLog
            {
                SlotId = 1,
                VehicleNumber = "ABC-123",
                CheckInTime = DateTime.Now.AddHours(-3),
                CheckOutTime = DateTime.Now,
                Fee = 150.00m,
                Status = "Completed"
            };
            var log2 = new ParkingLog
            {
                SlotId = 2,
                VehicleNumber = "XYZ-789",
                CheckInTime = DateTime.Now.AddHours(-2),
                CheckOutTime = DateTime.Now,
                Fee = 250.00m,
                Status = "Completed"
            };

            await _context.ParkingLogs.AddRangeAsync(log1, log2);
            await _context.SaveChangesAsync();

            var totalRevenue = await _parkingService.GetTotalRevenueAsync();
            Assert.That(totalRevenue, Is.EqualTo(400.00m));
        }
    }

}
