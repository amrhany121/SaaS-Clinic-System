namespace SaaS.Models
{
    public class Invoice : BaseEntity
    {
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public Guid? ConsultationId { get; set; } // الفاتورة قد تكون مرتبطة بكشف معين
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal NetAmount => TotalAmount - Discount + Tax;
        public string Status { get; set; } = "Unpaid"; // Paid, Unpaid, Partial

        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}
