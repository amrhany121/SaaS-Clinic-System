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
    [Authorize] // حماية البيانات بالتوكن
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : BaseController // الوراثة من الأب الجديد
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Doctors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
        {
            // TenantId دلوقت بييجي أوتوماتيك من الـ BaseController
            return await _context.Doctors
                .Where(d => d.TenantId == TenantId)
                .ToListAsync();
        }

        // GET: api/Doctors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(Guid id)
        {
            // البحث باستخدام معرف الطبيب ومعرف العيادة (الأمان)
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == TenantId);

            if (doctor == null)
            {
                return NotFound("الطبيب غير موجود في سجلات هذه العيادة.");
            }

            return doctor;
        }

        // PUT: api/Doctors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDoctor(Guid id, Doctor doctor)
        {
            if (id != doctor.Id) return BadRequest();

            // التأكد من ملكية السجل للعيادة الحالية قبل التعديل
            var exists = await _context.Doctors
                .AnyAsync(d => d.Id == id && d.TenantId == TenantId);

            if (!exists) return NotFound();

            // تثبيت الـ TenantId من التوكن لضمان عدم التلاعب
            doctor.TenantId = TenantId;
            _context.Entry(doctor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DoctorExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/Doctors
        [HttpPost]
        public async Task<ActionResult<Doctor>> PostDoctor(Doctor doctor)
        {
            // حقن معرف العيادة أوتوماتيكياً من هوية المستخدم
            doctor.TenantId = TenantId;

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDoctor", new { id = doctor.Id }, doctor);
        }

        // DELETE: api/Doctors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(Guid id)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == TenantId);

            if (doctor == null) return NotFound();

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DoctorExists(Guid id)
        {
            return _context.Doctors.Any(e => e.Id == id && e.TenantId == TenantId);
        }
    }
}