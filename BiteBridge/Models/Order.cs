using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace BiteBridge.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserEmail { get; set; } = "";

        public string Items { get; set; } = "";

        public int TotalPrice { get; set; }

        [NotMapped]
        public List<OrderItemView> ItemsList
        {
            get
            {
                return JsonSerializer.Deserialize<List<OrderItemView>>(Items) ?? new List<OrderItemView>();
            }
        }
    }

    public class OrderItemView
    {
        public string name { get; set; } = "";
        public int price { get; set; }
    }
}