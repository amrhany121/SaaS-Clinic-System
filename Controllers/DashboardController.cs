using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Data;

namespace SaaS.Controllers
{
    [Authorize] // حماية البيانات
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController // الوراثة من الأب لجلب الـ TenantId
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            // 1. عدد المرضى الكلي في العيادة
            var totalPatients = await _context.Patients
                .CountAsync(p => p.TenantId == TenantId);

            // 2. عدد مواعيد اليوم
            var today = DateTime.UtcNow.Date;
            var todayAppointments = await _context.Appointments
                .CountAsync(a => a.TenantId == TenantId && a.AppointmentDate.Date == today);

            // 3. إجمالي الدخل (الفواتير المدفوعة)
            var totalRevenue = await _context.Invoices
                .Where(i => i.TenantId == TenantId && i.Status == "Paid")
                .SumAsync(i => i.NetAmount);

            // 4. عدد الكشوفات المعلقة (مواعيد لم تتحول لكشوفات بعد)
            var pendingAppointments = await _context.Appointments
                .CountAsync(a => a.TenantId == TenantId && a.Status == "Scheduled");

            return Ok(new
            {
                TotalPatients = totalPatients,
                TodayAppointments = todayAppointments,
                TotalRevenue = totalRevenue,
                PendingAppointments = pendingAppointments,
                LastUpdate = DateTime.UtcNow
            });
        }

        [HttpGet("recent-patients")]
        public async Task<IActionResult> GetRecentPatients()
        {
            // آخر 5 مرضى تم تسجيلهم في العيادة
            var recentPatients = await _context.Patients
                .Where(p => p.TenantId == TenantId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            return Ok(recentPatients);
        }
    }
}