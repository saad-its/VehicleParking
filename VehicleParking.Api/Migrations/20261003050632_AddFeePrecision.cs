using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFeePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BookingTime",
                table: "ParkingSlots",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingTime",
                table: "ParkingSlots");
        }
    }
}
