using System.ComponentModel.DataAnnotations;

namespace BiteBridge.Models
{
    public class SystemLog
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string Level { get; set; } = "Info";

        [Required]
        public string Action { get; set; } = string.Empty;

        public string? UserEmail { get; set; }

        public string? Details { get; set; }
    }
}