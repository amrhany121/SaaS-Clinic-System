using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // أهم سطر لحل مشكلة ToListAsync
using SaaS.Models;

namespace SaaS.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SuperAdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperAdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("pending-clinics")]
        public async Task<IActionResult> GetPendingClinics()
        {
            // 1. هنجيب كل الـ Admins من جدول الأدوار مباشرة
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            // 2. هنفلترهم وناخد اللي IsActive بتاعهم false بس
            var pendingClinics = adminUsers
                .Where(u => !u.IsActive)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    userId = u.Id,
                    userName = u.UserName,
                    fullName = u.FullName,
                    email = u.Email,
                    requestDate = u.CreatedAt
                })
                .ToList();

            // 3. اطبع في الـ Console بتاع الفيجوال ستوديو عشان تتأكد
            Console.WriteLine($"عدد العيادات المعلقة اللي لقيناها: {pendingClinics.Count}");

            return Ok(pendingClinics);
        }
        [HttpPost("activate/{userId}")]
        public async Task<IActionResult> ActivateClinic(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("المستخدم غير موجود.");

            // 1. تفعيل الحساب
            user.IsActive = true;

            // 2. التأكد من وجود الدور قبل إضافته (عشان ميعملش Error)
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                    return BadRequest("فشل في إضافة صلاحيات المدير.");
            }

            // 3. حفظ التعديلات
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest("فشل في تحديث حالة الحساب.");

            return Ok(new { message = $"تم تفعيل حساب {user.FullName} بنجاح." });
        }
    }
}