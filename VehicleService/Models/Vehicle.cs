using System.ComponentModel.DataAnnotations;

namespace VehicleService.Models
{

    using System.ComponentModel.DataAnnotations.Schema;

    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        [Column("plate_number")]
        public string PlateNumber { get; set; } = string.Empty;

        [Column("machinery_type")]
        public string MachineryType { get; set; } = string.Empty;

        [Column("operational")]
        public bool IsOperational { get; set; } = true;

        [Column("fuel_capacity")]
        public double FuelCapacity { get; set; }

        [Column("registered_at")]
        public DateTime RegisteredAt { get; set; }

        [Column("fuel_consumption_per_km")]
        public double FuelConsumptionByKm { get; set; }

        [Column("model")]
        public string Model { get; set; } = string.Empty;
    }

}
