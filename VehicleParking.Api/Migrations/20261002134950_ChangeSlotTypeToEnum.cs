using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParking.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSlotTypeToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotType",
                table: "ParkingSlots");

            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                table: "ParkingSlots",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "ParkingSlots");

            migrationBuilder.AddColumn<string>(
                name: "SlotType",
                table: "ParkingSlots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
