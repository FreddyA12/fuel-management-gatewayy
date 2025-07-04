using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleService.Models
{
    public class Route
    {
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("origin")]
        public string Origin { get; set; } = string.Empty;

        [Column("destiny")]
        public string Destiny { get; set; } = string.Empty;

        [Column("distance_km")]
        public double DistanceKm { get; set; }

        [Column("est_consumption_per_km")]
        public double EstimatedConsumptionPerKm { get; set; }
    }
}
