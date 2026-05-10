namespace SaaS.Models
{
    public class Appointment : BaseEntity
    {
        public Guid PatientId { get; set; } // رقم المريض
        public Guid DoctorId { get; set; }  // رقم الطبيب

        public DateTime AppointmentDate { get; set; } // تاريخ الموعد
        public string Status { get; set; } = "Pending"; // (حجز، تم الكشف، ملغي)

        // ربط برمجي (Navigation Properties)
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}
