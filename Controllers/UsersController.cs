using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Models;

namespace SaaS.Controllers
{
    [Authorize] // حماية البيانات
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController // الوراثة من الأب لجلب الـ TenantId
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // 1. عرض كل الموظفين في العيادة الحالية
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Where(u => u.TenantId == TenantId)
                .Select(u => new {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. إضافة موظف جديد لنفس العيادة
        [HttpPost("add-staff")]
        public async Task<IActionResult> AddStaff([FromBody] AddStaffModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null) return BadRequest("هذا البريد الإلكتروني مسجل مسبقاً.");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                // السطر الأهم: ربط الموظف الجديد بنفس عيادة المدير الحالي
                TenantId = TenantId
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok("تم إضافة الموظف بنجاح للعيادة.");
            }

            return BadRequest(result.Errors);
        }

        // 3. حذف موظف من العيادة
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == TenantId);

            if (user == null) return NotFound("المستخدم غير موجود في عيادتك.");

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded) return Ok("تم حذف الموظف.");

            return BadRequest(result.Errors);
        }
    }

    // كلاس مساعد لإضافة موظف
    public record AddStaffModel(string Email, string Password, string FullName, string PhoneNumber);
}