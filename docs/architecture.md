# المخطط المعماري (Clean Architecture)

## 1) طبقات المشروع

- `src/Domain`
  - الكيانات الأساسية والقواعد الدومينية.
- `src/Application`
  - حالات الاستخدام (Use Cases)، الواجهات (Interfaces)، DTOs، والتحقق.
- `src/Infrastructure`
  - EF Core DbContext، إعدادات SQL Server، مستودعات التنفيذ، JWT Services.
- `src/API`
  - ASP.NET Core Web API Controllers / Minimal APIs، مصادقة JWT، التفويض بالأدوار.

## 2) قواعد Multi-Tenant و Multi-Branch

### قاعدة إلزامية
- كل جدول يجب أن يحتوي على:
  - `TenantId` (إجباري)
- كل بيانات التشغيل (مثل الطلبات، الشحنات، التحصيل، التتبع) يجب أن تحتوي أيضًا على:
  - `BranchId` (إجباري)

### تطبيق القاعدة تقنيًا
- إنشاء واجهة دومينية:
  - `ITenantEntity` فيها `Guid TenantId`
- إنشاء واجهة تشغيلية:
  - `IBranchEntity` ترث/تضيف `Guid BranchId`
- في `DbContext`:
  - `Global Query Filters` لعزل البيانات حسب `TenantId`
  - فرض التحقق من `BranchId` للكيانات التشغيلية قبل `SaveChanges`

## 3) تصميم الدومين المبدئي

## الكيانات المرجعية
- `Tenant`
  - `Id`, `Name`, `IsActive`
- `Branch`
  - `Id`, `TenantId`, `Name`, `Code`, `IsActive`
- `AppUser`
  - `Id`, `TenantId`, `UserName`, `PasswordHash`, `IsActive`
- `Role`
  - `Id`, `TenantId`, `Name`
- `UserRole`
  - `Id`, `TenantId`, `UserId`, `RoleId`

## الكيانات التشغيلية
- `Order`
  - `Id`, `TenantId`, `BranchId`, `CustomerName`, `Phone`, `Address`, `CreatedAt`
  - لا يوجد عمود State للمشاكل.
- `OrderFlag`
  - `Id`, `TenantId`, `BranchId`, `OrderId`, `FlagType`, `Notes`, `CreatedAt`, `IsResolved`

### لماذا Flags بدل State؟
- لأن المشكلة قد تكون متعددة على نفس الطلب (مثال: `AddressIssue` + `PhoneUnreachable`).
- يمكن إغلاق كل علم (Flag) بشكل مستقل دون تغيير الحالة التشغيلية الأساسية.

## 4) المصادقة والتفويض

- JWT Authentication:
  - Claims إلزامية داخل التوكن:
    - `sub` (UserId)
    - `tenant_id`
    - `branch_id` (عند تسجيل الدخول على فرع محدد)
    - `role`
- Role-Based Authorization:
  - سياسات مثل:
    - `RequireAdmin`
    - `RequireDispatcher`
    - `RequireCourier`

## 5) قاعدة البيانات (SQL Server + EF Core)

- استخدام EF Core Code First مع Migrations.
- Naming Convention واضحة للجداول والفهارس.
- فهارس مركبة مهمة:
  - `(TenantId, BranchId, CreatedAt)` للطلبات.
  - `(TenantId, OrderId, IsResolved)` لأعلام المشاكل.

## 6) واجهات عربية RTL

- أي طبقة واجهة مستقبلية (Portal/Admin Dashboard) يجب أن تكون:
  - اللغة الافتراضية: العربية
  - الاتجاه: `dir="rtl"`
  - تنسيقات تدعم RTL بالكامل

## 7) خارطة تنفيذ مرحلية

1. إنشاء Solution ومشاريع الطبقات الأربع.
2. بناء Domain entities + interfaces (`ITenantEntity`, `IBranchEntity`).
3. بناء DbContext مع Global Query Filters للـ Tenant.
4. بناء Auth (JWT) + سياسات الأدوار.
5. بناء Order + OrderFlag Use Cases.
6. بناء Endpoints الأساسية:
   - Auth/Login
   - Orders CRUD
   - Add/Resolve Flags
7. اختبارات تكامل أساسية.

## 8) DECISION REQUIRED

- آلية ربط المستخدم بفرع:
  - هل المستخدم يمكنه العمل على أكثر من فرع في نفس الجلسة أم فرع واحد لكل Token؟
- هل نستخدم ASP.NET Core Identity أم نظام User/Role مخصص بالكامل؟
- هل `OrderFlag` له قاموس ثابت (Enum) أم جدول مرجعي قابل للإدارة من لوحة التحكم؟
