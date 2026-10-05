using System;

namespace VehicleParking.Api.DTOs;

public class BookSlotDto
{
    public int UserId { get; set; }
    public int SlotId { get; set; }
    public required string VehicleNumber { get; set; }
}
