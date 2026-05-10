namespace SaaS.Models
{
    public class Doctor : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty; // التخصص
        public string Phone { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; } // سعر الكشف

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
