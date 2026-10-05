using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketAndFeeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketNumber",
                table: "ParkingSlots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "ParkingLogs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TicketNumber",
                table: "ParkingLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketNumber",
                table: "ParkingSlots");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "ParkingLogs");

            migrationBuilder.DropColumn(
                name: "TicketNumber",
                table: "ParkingLogs");
        }
    }
}
