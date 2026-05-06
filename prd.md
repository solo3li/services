🏗️ البنية التقنية

- ASP.NET Core  (MVC )
- sqlite
- Entity Framework Core
- SignalR wwithout redis






=============================
        القسم الأول: تطبيقات العميل 
=============================

=============================
        المستخدمين
=============================


الرول الأساسي عند التسجيل يكون **طالب (Student)** افتراضيًا.
ولو المستخدم يريد العمل كـ **منفذ (Executor)** أيضًا، لا يتم التحويل مباشرة — لازم يمر بمرحلة **توثيق الهوية KYC** ثم يتفعل له دور المنفذ.
يعني الحساب الواحد يمكن يكون:

* طالب فقط
* طالب + منفذ
* (ممنوع منفذ فقط عند البداية لو هذا قرارك التجاري)

وده أفضل من فصل الحسابات، لأن فصلهم فكرة ضعيفة وتسبب ازدواجية بيانات ومشاكل دعم.

---

# 👤 المستخدم الأساسي (Student - Default Role)

* تسجيل حساب
* تسجيل دخول
* إدارة الملف الشخصي
* تصفح الخدمات
* البحث والفلترة
* إنشاء طلب خدمة
* رفع ملفات داخل الطلب
* الدفع عبر Paymob لاحقا
* متابعة حالة الطلب
* شات داخل الطلب
* شات مباشر مع المنفذين
* فتح تذكرة دعم
* تأكيد استلام الخدمة
* تقييم المنفذ (لاحقًا)

---

# 👨‍💻 تفعيل دور المنفذ (Upgrade to Executor)

لو المستخدم ضغط: **أصبح منفذًا**

يظهر له:

* رفع بطاقة هوية / إثبات شخصية
* صورة شخصية
* بيانات الدفع / المحفظة
* نبذة تعريفية
* المهارات والخدمات التي يستطيع تنفيذها
* الموافقة على الشروط
* إرسال طلب التفعيل

### الحالات:

* Pending Review
* Approved
* Rejected
* Suspended

---

# 👨‍💻 بعد الموافقة يصبح الحساب: Student + Executor

ويكتسب صلاحيات إضافية:

* تصفح الطلبات المتاحة
* التقديم / قبول الطلبات
* تنفيذ الخدمات
* رفع ملفات التسليم
* شات داخل الطلب
* شات مباشر
* فتح تذاكر دعم
* متابعة الأرباح
* طلب سحب أرباح
* إحصائيات الأداء
* تقييمه بواسطة الطلاب

---
=============================
        الشاشات الأساسية
=============================

🔐 المصادقة
- Login
- Register
- OTP Verify
- Forgot Password

🏠 عامة
- Home
- Categories
- Service Details
- Search

📦 الطلبات
- Create Order
- My Orders
- Order Details
- Order Tracking

💬 الشات
- Conversations
- Direct Chat
- Order Chat

💳 الدفع
- Checkout
- Payment WebView
- Payment Result

🪪 المنفذ
- KYC Submit
- KYC Status
- Available Orders
- Earnings

🎫 الدعم
- Tickets List
- Open Ticket
- Ticket Details

👤 الحساب
- Profile
- Settings
- Logout

=============================
        القسم الثاني
=============================


=============================
        لوحة الأدمن
=============================

🛠️ إدارة النظام
- Dashboard
- إحصائيات المستخدمين
- إحصائيات الطلبات
- المدفوعات
- النزاعات

👥 إدارة المستخدمين
- عرض المستخدمين
- حظر / تفعيل
- تعديل صلاحيات
- مراجعة الحسابات

🪪 إدارة KYC
- مراجعة الطلبات
- قبول / رفض
- ملاحظات الرفض

📚 إدارة الخدمات
- إضافة خدمة
- تعديل خدمة
- حذف خدمة
- تفعيل / إيقاف

🗂️ إدارة التصنيفات
- CRUD كامل

⚙️ إعدادات التنفيذ
- من ينفذ الخدمة
- عمولة المنصة
- الأسعار الأساسية

📦 إدارة الطلبات
- عرض كل الطلبات
- تعيين منفذ
- تغيير الحالة
- التدخل اليدوي

💳 إدارة المدفوعات
- مراجعة عمليات Paymob
- تأكيد الدفع
- Refund يدوي لاحقًا

🎫 الدعم الفني
- استقبال التذاكر
- الرد على المستخدمين
- إغلاق النزاعات

=============================
         Modules
=============================

Controllers:
- AuthController
- UsersController
- ServicesController
- CategoriesController
- OrdersController
- PaymentsController
- ChatController
- TicketController
- KycController
- AdminController

Services:
- AuthService
- JwtService
- UserService
- KycService
- CatalogService
- OrderService
- PaymentService
- EscrowService
- ChatService
- FileService
- TicketService
- NotificationService

SignalR Hubs:
- ChatHub
- TicketHub
- NotificationHub








