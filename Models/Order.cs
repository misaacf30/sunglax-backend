namespace webapi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public decimal Total { get; set; }
        public string? StripeSessionId { get; set; }
        public DateTime? CreatedAt { get; set;}
        public bool? IsCompleted { get; set; }
        public string? Name { get; set; }
    }
}
