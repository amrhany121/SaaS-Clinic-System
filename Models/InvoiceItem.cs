namespace SaaS.Models
{
    public class InvoiceItem : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty; // اسم الخدمة (كشف، استشارة، تحليل)
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal SubTotal => UnitPrice * Quantity;
    }
}
