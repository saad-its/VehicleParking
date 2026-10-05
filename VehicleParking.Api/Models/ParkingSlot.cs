using System;

namespace VehicleParking.Api.Models;

public class ParkingSlot
{
    public int Id { get; set; }
    public required string SlotNumber { get; set; }
    public VehicleType VehicleType { get; set; }
    public bool IsAvailable { get; set; }
    public string? CurrentVehicleNumber { get; set; }
    public string? TicketNumber { get; set; }
    public DateTime? BookingTime { get; set; }
}
