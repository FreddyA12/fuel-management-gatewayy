using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehicleService.Models;
using DriverService.Models;
using RouteService.Models;


namespace FuelConsumption.Models
{
    [Table("fuel_consumption")]
    public class FuelConsumption
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("vehicle_id")]
        public int VehicleId { get; set; }

        [Required]
        [Column("route_id")]
        public int RouteId { get; set; }

        [Required]
        [Column("driver_id")]
        public int DriverId { get; set; }

        [Required]
        [Column("date")]
        public DateTime Date { get; set; }

        [Column("actual_consumption")]
        public double ActualConsumption { get; set; }

        [Column("state")]
        [StringLength(50)]
        public string? State { get; set; }

        // Relaciones (opcional si usas EF Navigation Properties)
        public Vehicle? Vehicle { get; set; }
        public RouteEntity? Route { get; set; }
        public Driver? Driver { get; set; }
    }
}
