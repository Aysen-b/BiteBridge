using System.ComponentModel.DataAnnotations;

namespace BiteBridge.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public int CatererId { get; set; }

        public Caterer? Caterer { get; set; }
    }
}