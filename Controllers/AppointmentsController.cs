using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Data;
using SaaS.Models;

namespace SaaS.Controllers
{
    [Authorize] // لا يسمح بالدخول إلا لمن يحمل Token سليم
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : BaseController // الوراثة من الأب الجديد
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            // نستخدم TenantId الموجودة في الـ BaseController (المستخرجة من الـ Token)
            return await _context.Appointments
                .Where(a => a.TenantId == TenantId)
                .ToListAsync();
        }

        // GET: api/Appointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(Guid id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == TenantId);

            if (appointment == null)
            {
                return NotFound("الموعد غير موجود أو ليس لديك صلاحية الوصول إليه.");
            }

            return appointment;
        }

        // PUT: api/Appointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointment(Guid id, Appointment appointment)
        {
            if (id != appointment.Id) return BadRequest();

            // التحقق من أن الموعد يخص العيادة صاحبة الـ Token
            var exists = await _context.Appointments
                .AnyAsync(a => a.Id == id && a.TenantId == TenantId);

            if (!exists) return NotFound();

            // ضمان عدم تزوير الـ TenantId أثناء التعديل
            appointment.TenantId = TenantId;
            _context.Entry(appointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/Appointments
        [HttpPost]
        public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
        {
            // حقن الـ TenantId آلياً من هوية المستخدم المسجل
            appointment.TenantId = TenantId;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAppointment", new { id = appointment.Id }, appointment);
        }

        // DELETE: api/Appointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(Guid id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == TenantId);

            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppointmentExists(Guid id)
        {
            return _context.Appointments.Any(e => e.Id == id && e.TenantId == TenantId);
        }
    }
}