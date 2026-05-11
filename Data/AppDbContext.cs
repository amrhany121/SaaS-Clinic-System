using Microsoft.EntityFrameworkCore;
using SaaS.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace SaaS.Data
{
    public class AppDbContext :  IdentityDbContext<ApplicationUser>
    {
        private readonly Guid _tenantId;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            // سحب الـ TenantId من التوكن
            var user = httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("TenantId")?.Value;

            if (Guid.TryParse(claim, out var tenantId))
            {
                _tenantId = tenantId;
            }
        }

        // الجداول الأساسية
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Consultation> Consultations { get; set; }

        // جداول الحسابات
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- إعدادات جدول Doctor ---
            modelBuilder.Entity<Doctor>()
                .Property(d => d.ConsultationFee)
                .HasColumnType("decimal(18,2)");

            // --- إعدادات جدول Patient ---
            modelBuilder.Entity<Patient>().HasIndex(p => p.Phone);
            modelBuilder.Entity<Patient>().HasIndex(p => p.NationalId);

            // --- إعدادات جدول Appointment (العلاقات) ---
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- إعدادات جداول المالية (Invoices) لضبط الـ Decimal ---
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(i => i.Discount).HasColumnType("decimal(18,2)"); // إضافة حقل الخصم
                entity.Property(i => i.Tax).HasColumnType("decimal(18,2)");      // إضافة حقل الضريبة
            });

            modelBuilder.Entity<InvoiceItem>()
                .Property(ii => ii.UnitPrice).HasColumnType("decimal(18,2)");


            // --- الـ Global Query Filters (سحر الـ SaaS) ---
            // السطور دي بتضمن إن أي مستخدم يشوف داتا العيادة بتاعته بس أوتوماتيكياً
            modelBuilder.Entity<Patient>().HasQueryFilter(p => p.TenantId == _tenantId);
            modelBuilder.Entity<Doctor>().HasQueryFilter(d => d.TenantId == _tenantId);
            modelBuilder.Entity<Appointment>().HasQueryFilter(a => a.TenantId == _tenantId);
            modelBuilder.Entity<Consultation>().HasQueryFilter(c => c.TenantId == _tenantId);
            modelBuilder.Entity<Invoice>().HasQueryFilter(i => i.TenantId == _tenantId);
            modelBuilder.Entity<InvoiceItem>().HasQueryFilter(ii => ii.TenantId == _tenantId);

        }
    }
}