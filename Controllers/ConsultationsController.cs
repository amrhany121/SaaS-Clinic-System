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
    [Authorize] // حماية الكشوفات، مفيش دخول من غير Token
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationsController : BaseController // الوراثة من الأب
    {
        private readonly AppDbContext _context;

        public ConsultationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Consultations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Consultation>>> GetConsultations()
        {
            // الفلترة بتتم أوتوماتيك باستخدام TenantId اللي في الـ BaseController
            return await _context.Consultations
                .Where(c => c.TenantId == TenantId)
                .ToListAsync();
        }

        // GET: api/Consultations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Consultation>> GetConsultation(Guid id)
        {
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == TenantId);

            if (consultation == null)
            {
                return NotFound("الكشف غير موجود أو لا تملك صلاحية الوصول إليه.");
            }

            return consultation;
        }

        // PUT: api/Consultations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConsultation(Guid id, Consultation consultation)
        {
            if (id != consultation.Id) return BadRequest();

            // التأكد من ملكية السجل للعيادة الحالية
            var exists = await _context.Consultations
                .AnyAsync(c => c.Id == id && c.TenantId == TenantId);

            if (!exists) return NotFound();

            consultation.TenantId = TenantId; // نثبت الـ TenantId من التوكن
            _context.Entry(consultation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsultationExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/Consultations
        [HttpPost]
        public async Task<ActionResult<Consultation>> PostConsultation(Consultation consultation)
        {
            // حقن المعرف الخاص بالعيادة آلياً
            consultation.TenantId = TenantId;

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConsultation", new { id = consultation.Id }, consultation);
        }

        // DELETE: api/Consultations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultation(Guid id)
        {
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == TenantId);

            if (consultation == null) return NotFound();

            _context.Consultations.Remove(consultation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConsultationExists(Guid id)
        {
            return _context.Consultations.Any(e => e.Id == id && e.TenantId == TenantId);
        }
    }
}