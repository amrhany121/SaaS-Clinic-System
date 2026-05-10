namespace SaaS.Models
{
    public class Patient : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? NationalId { get; set; } // الرقم القومي
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty; // ذكر / أنثى

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
