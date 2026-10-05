using System;

namespace VehicleParking.Api.Models;

public class User
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
