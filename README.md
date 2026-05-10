# 🏥 نظام إدارة العيادات (SaaS Backend)

مشروع Backend احترافي لإدارة العيادات الطبية بنظام الـ SaaS، حيث يدعم تعدد العيادات على قاعدة بيانات واحدة مع عزل تام للبيانات.

## كيف تبدأ؟
1. **قاعدة البيانات:** غير الـ Connection String في ملف `appsettings.json`.
2. **التهيئة:** افتح الـ Package Manager Console واكتب `Update-Database`.
3. **الأدوار:** الأدوار (SuperAdmin, Admin, Doctor) بتتكريت لوحدها أول ما تشغل البرنامج.

## نظام الأمان والصلاحيات
- **Multi-tenancy:** كل عيادة ليها `TenantId` خاص، ومستحيل عيادة تشوف بيانات التانية بفضل الـ Global Query Filter.
- **Activation:** أي دكتور بيسجل جديد بيكون حسابه "معلق" لحد ما الـ SuperAdmin يفعله.

## 📄 توثيق الـ API
بمجرد تشغيل المشروع، تقدر تشوف كل الـ Endpoints وتجربها من خلال الرابط:
`https://localhost:7092/swagger/index.html`
