using System.ComponentModel.DataAnnotations;

namespace VehicleService.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        public string PlateNumber { get; set; } = string.Empty;

        public string MachineryType { get; set; } = string.Empty;

        public bool IsOperational { get; set; } = true;

        public double FuelCapacity { get; set; }

        public DateTime RegisteredAt { get; set; }

        public double FuelConsumptionByKm { get; set; }

        public string Model { get; set; } = string.Empty;
    }
}
