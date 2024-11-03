namespace restaurant_demo_website.Models
{
    public class OrderDetail
    {
        public string Name {get;set;}
        public int OrderId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}