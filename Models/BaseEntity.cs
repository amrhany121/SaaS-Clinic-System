namespace SaaS.Models
{
    
    public abstract class BaseEntity
    {
        // المعرف الفريد لكل سجل
        public Guid Id { get; set; } = Guid.NewGuid();

        // أهم حقل في الـ SaaS: ده اللي بيميز بيانات عيادة (أ) عن عيادة (ب)
        public Guid TenantId { get; set; }

        // وقت إنشاء السجل
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // حمايتك من المسح النهائي (مسح منطقي)
        public bool IsDeleted { get; set; } = false;
    }
}
