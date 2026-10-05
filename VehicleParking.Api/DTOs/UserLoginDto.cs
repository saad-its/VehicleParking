using System;

namespace VehicleParking.Api.DTOs;

public class UserLoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
