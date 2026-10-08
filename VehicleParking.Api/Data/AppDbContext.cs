using System;
using Microsoft.EntityFrameworkCore;
using VehicleParking.Api.Models;

namespace VehicleParking.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<ParkingSlot> ParkingSlots { get; set; }
    public DbSet<ParkingLog> ParkingLogs { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ParkingLog>()
            .Property(l => l.Fee)
            .HasColumnType("decimal(18,2)");
    }
}