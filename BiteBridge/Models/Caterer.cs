using System.ComponentModel.DataAnnotations;

namespace BiteBridge.Models
{
    public class Caterer
    {
        public int Id { get; set; }

        [Required]
        public string BusinessName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Address { get; set; }

        public string? OwnerEmail { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}