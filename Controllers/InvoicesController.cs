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
    public class InvoicesController : BaseController // الوراثة من الأب
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Invoices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            // فلترة الفواتير حسب العيادة (Tenant) مع جلب بيانات المريض
            return await _context.Invoices
                .Where(i => i.TenantId == TenantId)
                .Include(i => i.Patient)
                .ToListAsync();
        }

        // GET: api/Invoices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(Guid id)
        {
            // جلب الفاتورة مع بنودها (Items) وبيانات المريض
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == TenantId);

            if (invoice == null)
            {
                return NotFound("الفاتورة غير موجودة أو لا تملك صلاحية الوصول إليها.");
            }

            return invoice;
        }

        // POST: api/Invoices
        [Authorize(Roles = "Admin,Secretary")]
        [HttpPost]
        public async Task<ActionResult<Invoice>> PostInvoice(Invoice invoice)
        {
            // حقن الـ TenantId أوتوماتيكياً في الفاتورة
            invoice.TenantId = TenantId;

            // ضمان حقن الـ TenantId في كل بند (Item) داخل الفاتورة للأمان
            if (invoice.Items != null)
            {
                foreach (var item in invoice.Items)
                {
                    item.TenantId = TenantId;
                }
            }

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoice", new { id = invoice.Id }, invoice);
        }

        // PUT: api/Invoices/5
        [Authorize(Roles = "Admin,Secretary")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoice(Guid id, Invoice invoice)
        {
            if (id != invoice.Id) return BadRequest();

            // التأكد من ملكية الفاتورة قبل التعديل
            var exists = await _context.Invoices
                .AnyAsync(i => i.Id == id && i.TenantId == TenantId);

            if (!exists) return NotFound();

            invoice.TenantId = TenantId;
            _context.Entry(invoice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Invoices/5
        [Authorize(Roles = "Admin,Secretary")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(Guid id)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == TenantId);

            if (invoice == null) return NotFound();

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceExists(Guid id)
        {
            return _context.Invoices.Any(e => e.Id == id && e.TenantId == TenantId);
        }
    }
}