using System;

namespace VehicleParking.Api.Models;

public class ParkingLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SlotId { get; set; }
    public required string VehicleNumber { get; set; }
    public DateTime CheckInTime { get; set; } = DateTime.Now;
    public DateTime? CheckOutTime { get; set; }
    public required string Status { get; set; }
    public string? TicketNumber { get; set; }
public decimal Fee { get; set; }
} 
