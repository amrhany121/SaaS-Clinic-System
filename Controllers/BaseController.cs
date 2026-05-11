using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SaaS.Controllers
{
    [ApiController]
    // لاحظ إننا مش بنحط [Route] هنا عشان هنسيب كل Controller يحدد طريقه
    public class BaseController : ControllerBase
    {
        // دي الخاصية اللي هنستخدمها في كل الـ Controllers التانية
        protected Guid TenantId
        {
            get
            {
                // بنروح للـ User (اللي جاي من التوكن) وبندور على الـ Claim اللي اسمه TenantId
                var claim = User.FindFirst("TenantId")?.Value;

                if (Guid.TryParse(claim, out var tenantId))
                {
                    return tenantId;
                }

                // لو التوكن بايظ أو الـ ID مش موجود بنرجع Guid فاضي للأمان
                return Guid.Empty;
            }
        }
    }
}
