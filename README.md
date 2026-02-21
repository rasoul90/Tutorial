# نظام SaaS لإدارة شركات التوصيل

هذا المستودع يحتوي على الهيكل التأسيسي لمشروع **SaaS** لإدارة شركات التوصيل مع الالتزام الصارم بالقيود التالية:

- .NET 8
- ASP.NET Core Web API
- Clean Architecture
- EF Core + SQL Server
- Multi-Tenant (TenantId إجباري في كل جدول)
- Multi-Branch (BranchId إجباري في كل بيانات التشغيل)
- JWT Authentication
- Role-Based Authorization
- مشاكل الطلب يتم تمثيلها كـ **Flags** وليس **State**
- الواجهات عربية بالكامل RTL

> ملاحظة: بيئة التنفيذ الحالية لا تحتوي على SDK الخاص بـ .NET، لذلك تم إعداد مخطط وهيكل تفصيلي قابل للتنفيذ فور توفر .NET 8.
