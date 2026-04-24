using System.ComponentModel.DataAnnotations;

namespace BiteBridge.Models
{
    public class MenuItemOption
    {
        public int Id { get; set; }

        [Required]
        public string OptionName { get; set; } = string.Empty;
        
        public string OptionType { get; set; } = string.Empty;

        public decimal ExtraPrice { get; set; }

        public int MenuItemId { get; set; }

        public MenuItem? MenuItem { get; set; }
    }
}