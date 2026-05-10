using Microsoft.AspNetCore.Identity;

namespace SaaS.Models
{
    public class ApplicationUser : IdentityUser
    {
        
            public string FullName { get; set; }
            public Guid TenantId { get; set; }

            // الحالة الافتراضية: غير مفعل حتى تراجعه أنت
            public bool IsActive { get; set; } = false;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
