namespace SaaS.Models
{
    public class Consultation : BaseEntity
    {
        public Guid AppointmentId { get; set; } // مرتبط بانهي موعد؟
        public virtual Appointment Appointment { get; set; } = null!;

        public string Diagnosis { get; set; } = string.Empty; // التشخيص
        public string Prescription { get; set; } = string.Empty; // الروشتة (الأدوية)
        public string? Notes { get; set; } // ملاحظات تانية
    }
}
