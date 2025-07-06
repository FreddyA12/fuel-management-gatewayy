using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RouteService.Models
{
    [Table("Routes")]
    public class RouteEntity
    {
        [Key]
        [Column("id")]
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

        [Column("start_lat")]
        public double StartLat { get; set; }

        [Column("start_lng")]
        public double StartLng { get; set; }

        [Column("end_lat")]
        public double EndLat { get; set; }

        [Column("end_lng")]
        public double EndLng { get; set; }
    }
}
