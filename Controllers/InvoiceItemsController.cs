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
    [Authorize] // حماية التوكن
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceItemsController : BaseController // الوراثة من الأب
    {
        private readonly AppDbContext _context;

        public InvoiceItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceItem>>> GetInvoiceItems()
        {
            // فلترة بنود الفاتورة حسب العيادة الحالية فقط
            return await _context.InvoiceItems
                .Where(ii => ii.TenantId == TenantId)
                .ToListAsync();
        }

        // GET: api/InvoiceItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceItem>> GetInvoiceItem(Guid id)
        {
            var invoiceItem = await _context.InvoiceItems
                .FirstOrDefaultAsync(ii => ii.Id == id && ii.TenantId == TenantId);

            if (invoiceItem == null)
            {
                return NotFound();
            }

            return invoiceItem;
        }

        // PUT: api/InvoiceItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceItem(Guid id, InvoiceItem invoiceItem)
        {
            if (id != invoiceItem.Id) return BadRequest();

            // التأكد من ملكية السجل للعيادة قبل التعديل
            var exists = await _context.InvoiceItems
                .AnyAsync(ii => ii.Id == id && ii.TenantId == TenantId);

            if (!exists) return NotFound();

            invoiceItem.TenantId = TenantId;
            _context.Entry(invoiceItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceItemExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/InvoiceItems
        [HttpPost]
        public async Task<ActionResult<InvoiceItem>> PostInvoiceItem(InvoiceItem invoiceItem)
        {
            // حقن الـ TenantId أوتوماتيكياً
            invoiceItem.TenantId = TenantId;

            _context.InvoiceItems.Add(invoiceItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceItem", new { id = invoiceItem.Id }, invoiceItem);
        }

        // DELETE: api/InvoiceItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceItem(Guid id)
        {
            var invoiceItem = await _context.InvoiceItems
                .FirstOrDefaultAsync(ii => ii.Id == id && ii.TenantId == TenantId);

            if (invoiceItem == null) return NotFound();

            _context.InvoiceItems.Remove(invoiceItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceItemExists(Guid id)
        {
            return _context.InvoiceItems.Any(e => e.Id == id && e.TenantId == TenantId);
        }
    }
}