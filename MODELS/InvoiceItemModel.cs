namespace ApiBilling.Models
{
    public class InvoiceItemModel
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount => Quantity * Rate;
    }
}
