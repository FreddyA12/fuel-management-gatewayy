using System.ComponentModel.DataAnnotations;

namespace DriverService.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string IdentificationNumber { get; set; } = string.Empty;

        public bool Available { get; set; } = true;

        [Required]
        public string MachineryType { get; set; } = string.Empty;
    }
}
