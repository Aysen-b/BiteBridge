using System.ComponentModel.DataAnnotations;

namespace BiteBridge.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Score { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}