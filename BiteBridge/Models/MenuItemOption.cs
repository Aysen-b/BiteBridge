using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiteBridge.Models
{
    public class MenuItemOption
    {
        public int Id { get; set; }

        [Required]
        public string OptionName { get; set; } = string.Empty;

        public string OptionType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraPrice { get; set; }

        public int MenuItemId { get; set; }

        public MenuItem? MenuItem { get; set; }
    }
}