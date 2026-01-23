# المرحلة 1 (التصميم النهائي المختصر)

> وثيقة موجزة لتثبيت قرارات المرحلة 1 قبل البدء بالتنفيذ البرمجي (بدون كود).

## 1) المعمارية (Architecture)
- **نمط معماري:** Clean Architecture / Modular MVC.
- **طبقات رئيسية:**
  - Presentation (MVC): Controllers, Views (Razor RTL), ViewModels, Filters.
  - Application: Services, Policies/Authorization, DTOs, Validators, Reports, Import/Export.
  - Domain: Entities, Enums, Value Objects (NormalizedName), Interfaces.
  - Infrastructure: EF Core, Identity, Security (JWT/HMAC/RateLimit), Encryption/Hashing, Audit, Excel.

## 2) هيكل المجلدات المقترح
```
/Rasad.Web
  /Controllers
  /Views
  /ViewModels
  /Filters
  /Resources (Localization .resx)
  /wwwroot (Bootstrap RTL, theme, icons)

/Rasad.Application
  /Services
  /DTOs
  /Policies
  /Validators
  /Reports
  /Export
  /Import

/Rasad.Domain
  /Entities
  /Enums
  /ValueObjects
  /Interfaces

/Rasad.Infrastructure
  /Persistence (DbContext, Migrations)
  /Identity
  /Security (Encryption, HMAC, JWT)
  /Audit
  /Excel
```

## 3) قاعدة البيانات (Schema مختصر)
### الجداول الأساسية
- **Ministries**: Id, NameAr, Code, IsActive.
- **MinistryDataWindows**: Id, MinistryId, StartAt, EndAt, IsActive, AfterCloseMessageAr, CreatedByUserId, CreatedAt.
- **Directorates / Departments / Sections / Units** مع علاقات هرمية.
- **Employees**:
  - بيانات الاسم: FullName, NormalizedName.
  - الرقم الوظيفي: JobNumber.
  - الرقم الوطني: NationalIdEncrypted + NationalIdHash.
  - علاقات الهيكل: MinistryId, DirectorateId, DepartmentId?, SectionId?, UnitId?.
  - التحقق: IsVerified, VerifiedAt?, VerifiedByUserId?, VerifiedSource.
  - التدقيق: CreatedByUserId, UpdatedByUserId, CreatedAt, UpdatedAt.
  - **Soft Delete**: IsDeleted, DeletedAt?, DeletedByUserId?.
- **UserScopes**: UserId, MinistryId, DirectorateId, DepartmentId?, SectionId?, UnitId?, CanImportExcel, CanExportExcel, CanViewNationalIdFull.
- **AuditLogs**: تفاصيل العمليات الحساسة (Create/Update/Delete/Import/Export/Verify/Login… إلخ).
- **ImportBatches**: تتبع استيراد Excel (عدد السجلات، نجاح/فشل، ملف أخطاء… إلخ).
- **ApiClients**: ClientId, SecretHash, IsActive, MinistryId?, AllowedIps?, CreatedAt.
- **ApiNonces**: ClientId, Nonce, Timestamp, ExpiresAt, IsUsed (Unique(ClientId, Nonce) + TTL).

### القيود والفهارس الرئيسية
- **تفرد الرقم الوظيفي داخل الوزارة:** Unique(MinistryId, JobNumber).
- **فهرسة للكشف والتقارير:**
  - Employees: (NationalIdHash), (MinistryId, NationalIdHash), (NationalIdHash, NormalizedName, MinistryId), (MinistryId, JobNumber) UNIQUE.
  - فهارس نطاقية للتقارير: (MinistryId, DirectorateId, DepartmentId, SectionId, UnitId).
  - INCLUDE للتقارير: FullName, NormalizedName, IsVerified, JobNumber, CreatedAt.

## 4) سياسات الصلاحيات (Authorization)
- **Roles:** SuperAdmin, MinistryAdmin, MinistryUser.
- **Policies:**
  1) MinistryScopePolicy (نطاق الوزارة).
  2) OrgUnitScopePolicy (نطاق الهيكل الإداري).
  3) WindowTimePolicy (نافذة الإدخال).
  4) ExcelPermissionPolicy (استيراد/تصدير).
  5) CanViewNationalIdFullPolicy (عرض الرقم الوطني كاملاً).
  6) VerificationApiPolicy (JWT + HMAC + Nonce + RateLimit).

## 5) قرارات نهائية مُضافة (حسب طلبك)
### 5.1 Soft Delete
- الحذف **Soft فقط** (IsDeleted/DeletedAt/DeletedBy).
- التقارير **تستبعد المحذوف افتراضيًا**.
- **SuperAdmin** يمكنه عرض المحذوف عبر فلتر.

### 5.2 WindowTimePolicy
- قيود نافذة الإدخال تُطبّق على **MinistryAdmin** و**MinistryUser**.
- **SuperAdmin مستثنى** ويستطيع التعديل دائمًا.

### 5.3 Scope + Export
- كل استعلام (UI/تقارير/تصدير Excel) يطبّق **نفس نطاق الصلاحيات** (وزارة/دائرة/قسم/شعبة/وحدة) دون استثناءات.
- **صلاحية عرض الرقم الوطني** تؤثر على جميع المخرجات (UI + تقارير + Excel).

### 5.4 سياسة تطبيع الأسماء (Normalization) — مرجع ثابت
- **تنظيف المسافات:** Trim + تحويل المسافات المتعددة لمسافة واحدة.
- **إزالة التطويل:** حذف جميع أحرف المد (ـ).
- **توحيد الألف:** أ/إ/آ → ا.
- **توحيد الياء:** ى → ي.
- **توحيد التاء المربوطة:** ة → ه (مُعتمد للتطابق والكشف).
- هذه السياسة ثابتة وتُستخدم في: التقارير، الكشف عن الوهمي، الازدواج الوظيفي، والفهارس المعتمدة على NormalizedName.

### 5.5 سياسة تعارض الاستيراد (MVP)
- عند وجود تكرار في **NationalIdHash** أو **(MinistryId, JobNumber)**: يُعتبر السطر **خطأ** ويُسجّل في ملف أخطاء.
- **لا يوجد Update/Upsert في MVP** إلا إذا تم تفعيل خيار صريح لاحقًا لأدمن الوزارة فقط.

## 6) الترجمة والتعريب 100%
- جميع النصوص عبر ملفات **.resx** (UI + Validation + Identity + رسائل الاستيراد والتصدير).
- Culture افتراضي `ar-IQ` أو `ar-SA` مع RTL دائم.
- لا يظهر أي نص إنجليزي في الواجهة أو الرسائل أو ملفات Excel.

## 7) خطة MVP (مختصر)
1) إنشاء مشروع ASP.NET Core MVC 9 مع RTL وتعريب كامل.
2) Identity + Roles + Users Management.
3) إدارة الوزارات والهيكل الإداري.
4) CRUD الموظفين مع تطبيق النطاق + نافذة الإدخال.
5) API التحقق (JWT + HMAC + Nonce + RateLimit).
6) Dashboard + تقارير + Excel Export.
7) Excel Import مع تقرير أخطاء.
8) Audit Log + تحسينات الأمان.

## 8) قرارات سلامة بيانات إضافية (نهائية)
### 8.1 اتساق الهيكل الإداري داخل Employees
- يُفرض اتساق هرمي على حقول Employee التنظيمية:
  - إذا UnitId موجود ⇒ يجب وجود SectionId وDepartmentId وDirectorateId وأن تكون جميعها متسقة وتابعة لنفس الوزارة.
  - إذا SectionId موجود ⇒ يجب وجود DepartmentId وDirectorateId متسقة.
  - إذا DepartmentId موجود ⇒ يجب وجود DirectorateId متسقة.
  - جميع المستويات يجب أن تنتمي لنفس MinistryId.

### 8.2 سياسة تكرار الرقم الوطني داخل الوزارة
- يمنع تكرار الرقم الوطني داخل نفس الوزارة:
  - Unique منطقي على (MinistryId, NationalIdHash) للسجلات غير المحذوفة.
- أي محاولة إدخال تكرار تُرفض وتُسجل في AuditLogs.

### 8.3 تقييد عميل API التحقق بالوزارة
- إذا ApiClient مرتبط بـ MinistryId محددة:
  - يُسمح له بالتحقق فقط داخل نفس الوزارة.
  - أي طلب يحاول التحقق خارجها يُرفض ويُسجل في AuditLogs.
