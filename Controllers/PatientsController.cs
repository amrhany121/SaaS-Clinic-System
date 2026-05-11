using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using SaaS.Data;
using SaaS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaaS.Controllers
{
    [Authorize] // حماية بيانات المرضى (أهم جدول في السيستم)
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : BaseController // الوراثة من الأب
    {
       
        private readonly AppDbContext _context;

        // 2. عدل الـ Constructor عشان يستلم الـ context بس
        public PatientsController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/Patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
        {
            // جلب مرضى العيادة دي بس بناءً على التوكن
            return await _context.Patients
                .Where(p => p.TenantId == TenantId)
                .ToListAsync();
        }

        // GET: api/Patients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(Guid id)
        {
            // البحث بالـ Id والـ TenantId لضمان العزل التام
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == TenantId);

            if (patient == null)
            {
                return NotFound("المريض غير موجود أو لا تملك صلاحية الوصول إليه.");
            }

            return patient;
        }

        // PUT: api/Patients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPatient(Guid id, Patient patient)
        {
            if (id != patient.Id) return BadRequest();

            // التأكد من أن المريض يخص العيادة قبل السماح بالتعديل
            var exists = await _context.Patients
                .AnyAsync(p => p.Id == id && p.TenantId == TenantId);

            if (!exists) return NotFound();

            // تثبيت الـ TenantId من التوكن (أهم خطوة أمان)
            patient.TenantId = TenantId;
            _context.Entry(patient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/Patients
        [Authorize(Roles = "Admin,Secretary")]
        [HttpPost]
        public async Task<ActionResult<Patient>> PostPatient(Patient patient)
        {
            // حقن معرف العيادة تلقائياً
            patient.TenantId = TenantId;

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPatient", new { id = patient.Id }, patient);
        }

        // DELETE: api/Patients/5
        [Authorize(Roles = "Admin,Secretary")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == TenantId);

            if (patient == null) return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

      
        [HttpGet("stats")]
        public async Task<IActionResult> GetClinicStats()
        {
            // هنا بنعد المرضى اللي في العيادة دي (الفلتر شغال أوتوماتيك)
            var totalPatients = await _context.Patients.CountAsync();

            // عدد مرضى النهاردة
            var todayPatients = await _context.Patients
                .CountAsync(p => p.CreatedAt.Date == DateTime.Today);

            return Ok(new
            {
                TotalPatients = totalPatients,
                TodayPatients = todayPatients,
                LastUpdated = DateTime.Now
            });
        }
        private bool PatientExists(Guid id)
        {
            return _context.Patients.Any(e => e.Id == id && e.TenantId == TenantId);
        }
    }
}