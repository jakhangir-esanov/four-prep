# FourPrep

## Lokal ishga tushirish

Kerak bo'ladi: .NET 10 SDK, Docker va `localhost:5432` da ishlab turgan PostgreSQL (`postgres` / `postgres`).

1. Keycloak uchun bazani bir marta yarating:

   ```bash
   psql -h localhost -U postgres -c "CREATE DATABASE fourprep_keycloak"
   ```

2. Keycloak, Mailpit, fayl ombori (SeaweedFS) va ClamAV'ni ishga tushiring:

   ```bash
   docker compose up -d --build
   ```

   Keycloak rasmiy image emas: `keycloak/Dockerfile` unga Google orqali kirish uchun extension qo'shadi (pastda "Google orqali kirish" bo'limi). Birinchi yig'ish Maven bilan bir necha daqiqa oladi va extension'ning unit testlarini ham ishga tushiradi.

   Birinchi ishga tushishda `keycloak/realms/fourprep-realm.json` dagi `fourprep` realm'i import qilinadi. Unda 7 ta rol, `fourprep-api` client'i (audience va `country` mapper'lari, service account huquqlari bilan), token muddatlari, tillar (en, uz, ru), SMTP sozlamasi, User Profile, `google` identity provider'i va `fourprep direct grant` oqimi bor. Realm allaqachon mavjud bo'lsa, import o'tkazib yuboriladi. Fayldagi o'zgarishni qo'llash uchun `fourprep_keycloak` bazasini o'chirib, qayta yarating.

3. API'ni ishga tushiring:

   ```bash
   dotnet run --project src/API/FourPrep.Api --launch-profile http
   ```

   Swagger: http://localhost:5198/swagger. Migratsiyalar ishga tushishda avtomatik qo'llanadi.

## Faqat lokal muhit uchun ma'lumotlar

| Nima | Qiymat |
|---|---|
| Keycloak admin konsoli | http://localhost:8080, `admin` / `admin` |
| `super_admin` foydalanuvchi | `superadmin` / `superadmin` |
| `fourprep-api` client secret | `fourprep-dev-secret` |
| Mailpit (yuborilgan xatlar) | http://localhost:8025 |
| Fayl ombori (SeaweedFS S3) | http://localhost:8333, `fourprep-dev` / `fourprep-dev-storage-secret`, yopiq bucket `fourprep-documents`, ochiq bucket `fourprep-public` |
| Virus tekshiruvi (ClamAV) | `localhost:3310` |

Bu qiymatlarni prod'da ishlatmang.

## Modullar

Har bir modul o'z PostgreSQL sxemasiga ega va boshqa modulga faqat adapter orqali murojaat qiladi (`CLAUDE.md`, 3.3).

| Modul | Nima uniki | Holati |
|---|---|---|
| Auth | Keycloak hisoblari, login, sessiyalar, o'z hisobi, xodimlar hisoblari | tayyor |
| Student | o'quvchi profili, ota-ona bog'lanishi, mentor; keyinroq o'quvchining bosqichi (stage) va yo'li, test ballari, shaxsiy hujjatlari va offboarding | profil, ota-ona va mentor tayyor |
| Catalogue | universitetlar, dasturlar, qabul talablari, narxlar, muddatlar, grantlar, yo'nalish davlatlari | tayyor: qo'lda kiritish, CSV import, ochiq qidiruv |
| Content | yangiliklar, natijalar (results), hamkorlar, About, xodimlarning ochiq profillari | yangiliklar tayyor: muqova rasmi, e'lon qilish, ochiq o'qish |
| Academy | kurslar, guruhlar, yozilish, davomat, mock ballar, placement test | kurslar, guruhlar, yozilish va navbat tayyor (to'lovsiz) |
| Admission | academy arizasi, universitetlarga arizalar, portal akkauntlari, essay, tavsiyanomalar, universitet offerlari, viza | academy arizasi tayyor: yuborish, qaytarib olish, ko'rib chiqish |
| Agreement | mahsulotlar, offer va chegirmalar, shartnomalar, imzolar | skeleton |
| Payment | invoice, to'lovlar, to'lov tizimining webhook'i | skeleton |
| Meeting | konsultatsiyalar, xodimlarning bo'sh vaqtlari, uchrashuv havolalari (Zoom/Meet), mock interview | konsultatsiya tayyor: xodim jadvali, bo'sh vaqtlar, bron, bekor qilish, qatnashuv |
| AI | AI Counselor, plan wizard va My Plan, haftalik check-in | skeleton |
| Chat | suhbatlar va xabarlar | mentor va support thread'lari tayyor: xabar yuborish, o'qish, o'qilmaganlar, xodim navbati |
| Notification | ilova ichidagi, email va Telegram bildirishnomalari | skeleton |
| Storage | faqat fayl baytlari: obyektlar, yuklash va yuklab olish uchun imzolangan havolalar, o'chirish | tayyor: yuklash, tasdiqlash, ClamAV tekshiruvi, yuklab olish, boshqa modullar uchun API |
| Audit | barcha modullardagi muhim amallarning faqat qo'shiladigan (append-only) jurnali; hech qachon o'chirilmaydi | skeleton |

Nega shunday bo'lindi:

- **Catalogue Content'dan alohida.** AI universitetlar haqida faqat katalogdagi faktlarga tayanadi, har bir faktning manbasi (`source_url`) va tekshirilgan sanasi bor, eskirsa ogohlantiriladi. Yangilik va natijalarda bunday qoidalar yo'q. Ikkalasini `content_admin` boshqaradi, lekin ma'lumot va qoidalar har xil, AI esa faqat katalogga adapter orqali murojaat qiladi. Bitta Content moduli ham ko'rib chiqildi: unda AI yangiliklar bilan bir modulga bog'lanib qolardi.
- **Hujjatlar Student'da.** Pasport, transkript, sertifikat o'quvchining yozuvi: turi, holati (yuklangan, tasdiqlangan, rad etilgan), kim tekshirgani va qachon o'chirilishi Student'da saqlanadi. Storage faqat baytlarni saqlaydi va fayl nima ekanini bilmaydi, Student hujjati Storage'dagi fayl id'siga ishora qiladi. Offboarding'da hujjatlarni o'chirishni ham Student boshqaradi: yozuv qoladi, fayl o'chadi. Hujjatlarni Storage'ga yoki alohida modulga berish ko'rib chiqildi: Storage texnik ombor bo'lib qolishi kerak, alohida modul esa offboarding'ni ikki modulga bo'lib yuborardi.
- **Meeting alohida.** Konsultatsiyani Admission, mock interview'ni Admission va Academy, darslarni Academy ishlatadi. Uchrashuv, xodimning bo'sh vaqti va uchrashuv havolasi (Zoom yoki Google Meet, video platformada emas) bitta joyda turadi, qolgan modullar faqat uchrashuv id'sini saqlaydi.
- **Audit alohida.** Jurnal hech qachon o'chirilmaydi va boshqa modullarning offboarding'i unga tegmaydi. Modullar unga qanday yozishi (event yoki adapter) Audit qurilayotganda hal qilinadi.

Foydalanuvchi paneli mehmon uchun ham ishlaydi: katalog, kurslar, yangiliklar kabi umumiy ma'lumotni o'qiydigan endpoint'lar login talab qilmaydi. Shaxsiy ma'lumot va o'zgartirish uchun esa login kerak.

## Keycloak bo'yicha eslatmalar

- **User Profile.** Keycloak 26 standart holatda `firstName` va `lastName` ni majburiy qiladi. Ism-familiyasiz yaratilgan foydalanuvchi login paytida `Account is not fully set up` xatosini oladi. Realm faylida bu ikki maydon ixtiyoriy qilingan.
- **Atributlar.** `phone_number`, `country` va `zoneinfo` User Profile'da e'lon qilingan. E'lon qilinmagan atributni Keycloak hech qanday xatosiz tashlab yuboradi. `locale` alohida e'lon qilinmaydi: realm'da tillar yoqilgani uchun Keycloak uni o'zi boshqaradi.
- **Rollar.** `POST /auth/register` orqali ro'yxatdan o'tgan foydalanuvchi `student` rolini oladi, `POST /auth/register-parent` orqali o'tgan esa `parent` rolini. Xodimlarni `super_admin` `POST /admin/users` orqali kerakli rol bilan yaratadi.
- **Parol siyosati va bloklash.** Parol 8–128 belgi bo'lishi, username va email manzil bilan bir xil bo'lmasligi kerak; rad etilgan parol `400 User.PasswordNotAllowed` qaytaradi. Uzunlik va email qoidasini Keycloak (`passwordPolicy`) tekshiradi. Username qoidasini esa API ro'yxatdan o'tishda va `PUT /auth/me/password` da tekshiradi: Keycloak'ning `notUsername` qoidasi lokal `superadmin`/`superadmin` foydalanuvchisi tufayli realm importini buzardi. 10 ta noto'g'ri urinishdan keyin hisob vaqtincha bloklanadi (1 daqiqadan boshlab, ko'pi bilan 15 daqiqa). Bloklangan hisob ham oddiy `Authentication.InvalidCredentials` oladi, shuning uchun bloklash hisob borligini oshkor qilmaydi. Shu sababli birov xodim hisobini ataylab 15 daqiqagacha bloklab qo'yishi mumkin.
- **Sessiyalar.** Har bir login (parol, Google yoki ro'yxatdan o'tish) `auth.user_sessions` da bitta yozuv ochadi va unga Keycloak sessiyasining `sid` i yoziladi. `POST /auth/refresh` yangi qator qo'shmaydi: shu `sid` li yozuvda access token hash'i, muddati va `refreshed_on_utc` yangilanadi. Bekor qilingan (revoked) sessiyani refresh qilib bo'lmaydi. `POST /auth/logout-all` avval Keycloak'dagi barcha sessiyalarni yopadi, keyin yozuvlarni bekor qiladi, shuning uchun hamma qurilmadagi refresh token'lar ishlamay qoladi. `POST /auth/logout` faqat shu refresh token'ning Keycloak sessiyasini yopadi va uning yozuvini bekor qiladi. Allaqachon berilgan access token esa muddati tugaguncha (30 daqiqa) amal qiladi: API har so'rovda sessiya yozuvini tekshirmaydi. Hangfire har kuni muddati 30 kundan oldin tugagan yozuvlarni o'chiradi (`auth-purge-expired-sessions`): Keycloak sessiyasi 30 kun ishlatilmasa yopiladi.

## O'quvchi, ota-ona va mentor

- **Profil.** `student` rolini olgan har bir hisob (o'zi ro'yxatdan o'tgan yoki admin yaratgan) uchun Student modulida bo'sh profil va 8 belgili bog'lash kodi ochiladi. Profil yaratilmasa, Keycloak hisobi ham o'chiriladi.
- **Ota-onani bog'lash.** O'quvchi kodni `GET /students/me` dan ko'radi va ota-onasiga beradi. Ota-ona kodni `POST /parents/me/students` ga yuboradi. Bitta o'quvchiga ko'pi bilan 4 ta ota-ona bog'lanadi. O'quvchi `POST /students/me/link-code` bilan yangi kod olsa, eski kod ishlamay qoladi. Kodni faqat o'quvchining o'zi ko'radi: admin uchun `GET /students/{id}` va ro'yxatlarda `linkCode` maydoni yo'q, chunki kodga ega har qanday ota-ona hisobi o'zini o'quvchiga bog'lay oladi.
- **Mentor.** `mentor_admin` `PUT /students/{id}/mentor` bilan faqat `mentor` rolidagi foydalanuvchini biriktira oladi. Mentor o'z o'quvchilarini `GET /mentors/me/students` orqali ko'radi.
- **Modullararo chaqiruvlar.** Ular tofan'dagidek adapter loyihalar orqali o'tadi: `FourPrep.Modules.Auth.Student` (register → profil) va `FourPrep.Modules.Student.Auth` (mentor rolini tekshirish).

## Universitetlar katalogi (Catalogue)

Katalog AI Counselor va My Plan uchun yagona fakt manbasi: AI universitet haqida faqat shu yerdagi ma'lumotni aytadi. Shuning uchun har bir fakt guruhi o'z manbasi (`sourceUrl`) va tekshirilgan sanasi (`checkedOn`) bilan saqlanadi. Ma'lumotni `content_admin` (va `super_admin`) staff panelda qo'lda kiritadi yoki CSV fayllardan ommaviy yuklaydi (pastda "CSV import").

**Tuzilishi.**

| Aggregate | Ichidagilar | Nima |
|---|---|---|
| `Destination` | — | 4Prep ariza xizmatini ko'rsatadigan davlat va uning viza turi (`F-1`, `National visa (D)`) |
| `University` | `Cost`, `Scholarship` | universitet; o'quv yili bo'yicha narxlar va grantlar |
| `Program` | `AdmissionRequirement`, `Deadline` | dastur (daraja, yo'nalish, o'qitish tili); qabul mavsumi bo'yicha talablar va raund muddatlari |

**Endpoint'lar.**

- Ochiq, login talab qilmaydi (mehmon ham ko'radi), bitta IP'dan daqiqasiga 120 so'rov (`RateLimitPolicies.PublicRead`, oshsa `429`):
  - `GET /universities?q=&country=&degree=&major=&budget=&currency=&sat=&partner=&serviceDestinationsOnly=&first=&rows=&sortField=&sortOrder=` — sahifalangan ro'yxat, bir sahifada ko'pi bilan 100 ta (standart 20). Saralash: `name` (standart), `ranking`, `acceptance_rate`, `checked_on`, `is_stale` va boshqa ustunlar.
  - `GET /universities/{slug}` — universitet, uning dasturlari (talablar va muddatlar bilan), narxlari va grantlari.
  - `GET /countries` — universiteti bor yoki xizmat yo'nalishi bo'lgan davlatlar: universitetlar soni, `isServiceDestination`, `visaType`.
  - `GET /majors?q=` — dasturlardagi yo'nalishlar va dasturlar soni (ko'pi bilan 20 ta).
- `content_admin` va `super_admin` (`Policies.ContentAdmin`):
  - `PUT /admin/destinations/{countryCode}` `{ "visaType" }` — davlatni xizmat yo'nalishi qiladi yoki viza turini o'zgartiradi; `DELETE` ro'yxatdan olib tashlaydi.
  - `POST /admin/universities`, `PUT` / `DELETE /admin/universities/{id}`.
  - `PUT /admin/universities/{id}/costs`, `PUT /admin/universities/{id}/scholarships`.
  - `POST /admin/universities/{universityId}/programs`, `PUT` / `DELETE /admin/programs/{id}`.
  - `PUT /admin/programs/{id}/requirements`, `PUT /admin/programs/{id}/deadlines`.
  - CSV import: `POST /admin/universities/import`, `/admin/programs/import`, `/admin/costs/import`, `/admin/scholarships/import`, `/admin/requirements/import`, `/admin/deadlines/import` (`multipart/form-data`, maydon nomi `file`).

**Qoidalar va qarorlar.**

- **Manba va eskirish.** Har bir universitet, dastur, narx, grant, talab va muddat yozuvida `sourceUrl` (to'liq `http(s)` manzil) va `checkedOn` (kelajakda bo'lmaydi) majburiy. 12 oydan eski fakt javobda `isStale: true` bilan keladi va frontend "eskirgan bo'lishi mumkin" belgisini ko'rsatadi. Belgi har so'rovda bugungi sanadan hisoblanadi, shuning uchun spec'dagi haftalik `stale.check` job'i kerak emas. Har bir yozuvda uni oxirgi marta kim tekshirgani (`checkedByUserId`) saqlanadi.
- **Ma'lumot yo'q — "not reported".** SAT siyosati, GPA, insholar soni, intervyu kabi maydonlar `null` bo'lishi mumkin: `null` "ma'lum emas" degani, `false` yoki `0` emas. AI va frontend uni taxmin qilmaydi.
- **Ichki ro'yxatlar to'liq almashtiriladi.** Narxlar, grantlar, talablar va muddatlar alohida `PUT` bilan butun ro'yxat sifatida yuboriladi; yuborilmagani o'chadi. Staff panelda bu jadvallar universitet yoki dastur sahifasida bitta forma bo'lib turadi. Universitet o'chirilsa, uning dasturlari ham o'chadi.
- **Ko'p tilli matn.** Tavsif, grant miqdori va shartlari `{ "en": "...", "uz": "...", "ru": "..." }` ko'rinishida bitta `jsonb` ustunda saqlanadi. Matn bo'lsa, inglizchasi majburiy; boshqa tillar ixtiyoriy. Javobda hamma tillar keladi, frontend foydalanuvchi tilini, u bo'lmasa inglizchasini ko'rsatadi (xato `messages` bilan bir xil yondashuv). Yangi til qo'shish uchun ustun qo'shish shart emas. Universitet va dastur nomi tarjima qilinmaydi: ular rasmiy nomi bilan yoziladi.
- **Davlat nomlari saqlanmaydi.** Davlat ISO 3166-1 alpha-2 kodi bilan yuradi, nomini frontend o'z tilida chiqaradi (`Intl.DisplayNames`). Shu sabab spec'dagi `countries.name_i18n` jadvali yo'q.
- **Xizmat yo'nalishlari kodga yozilmagan.** Ko'rish butun dunyo bo'yicha, xizmat davlatlari esa admin tomonidan `PUT /admin/destinations/...` bilan qo'shiladi. Migratsiya hech qanday davlatni seed qilmaydi: AQSh (F-1) va Yevropa davlatlari ro'yxatini 4Prep o'zi kiritadi.
- **Pul.** Narxlar valyutaning eng kichik birligida butun son (`tuitionMinor`, `livingMinor`, masalan sent) va ISO 4217 valyuta kodi bilan. O'quv yili boshlanish yili bilan yoziladi (2026/27 → `2026`).
- **Filtrlar.** `budget` faqat `currency` bilan ishlaydi va universitetning eng so'nggi o'quv yilidagi (to'lov + yashash) narxi shu valyutada va shu summadan oshmasa mos keladi; valyutalar konvertatsiya qilinmaydi. `sat` — SAT talab qilinmaydigan, yoki talab qilinsa o'rtacha 50% oralig'ining pastki chegarasi shu balldan oshmaydigan (yoki noma'lum) dasturi bor universitetlar. `degree`, `major` va `sat` bir dasturga birga qo'llanadi.
- **Talablar.** Ingliz tili minimumi `{ "Ielts": 6.5, "Toefl": 90, "Duolingo": 120 }` — faqat shu uch test, har biri o'z shkalasida. SAT o'rtacha 50% oralig'i ikkita to'g'ri SAT bali, avval kichigi. GPA o'z shkalasi bilan (masalan 4.0 shkalada 3.5), chunki davlatlarda shkala har xil.
- **Yo'nalish (`field`) erkin matn.** `GET /majors` dasturlarda yozilgan yo'nalishlardan qidiradi. Standart klassifikator (masalan ISCED-F) keyin, ma'lumot ko'paygach ko'rib chiqiladi.
- **Logo** hozircha yo'q: Storage moduli qurilgach, fayl id'si qo'shiladi.
- **Takrorlar.** Bir o'quv yiliga ikkita narx, bir dasturda ikki bir xil raund yoki qabul mavsumi validator'da rad etiladi. Ichki jadvallarda unique indeks yo'q: ro'yxat almashtirilganda eski qatorlar o'chib, yangilari shu saqlashda yoziladi. Dastur nomi esa universitet ichida unique (`Program.NameTaken`, bazada ham indeks), chunki import dasturni universitet slug'i va nomi bilan topadi.
- **jsonb o'qish.** Ko'p tilli matn (`Dictionary<string, string>`) uchun Dapper JSON type handler'ini Common (`AddInfrastructure`) qo'shadi, chunki uni Academy ham ishlatadi. `CatalogueModule` faqat `Dictionary<Exam, decimal>` handler'ini qo'shadi. Dapper handler'lari butun jarayon uchun umumiy: boshqa modul shu turlarni Dapper bilan o'qisa, ular ham JSON deb o'qiladi.

### CSV import

Har bir jadval o'z faylida yuklanadi. Tartib: avval universitetlar, keyin dasturlar, so'ng qolganlari (ular universitet yoki dasturga slug va nom orqali bog'lanadi).

| Endpoint | Majburiy ustunlar | Ixtiyoriy ustunlar | Kalit va nima bo'ladi |
|---|---|---|---|
| `/admin/universities/import` | `slug`, `name`, `country_code`, `source_url`, `checked_on` | `city`, `website`, `ranking`, `ranking_source`, `acceptance_rate`, `is_partner`, `description_<til>` | `slug`: bor bo'lsa yangilanadi, yo'q bo'lsa yaratiladi |
| `/admin/programs/import` | `university_slug`, `name`, `degree_level`, `field`, `language`, `source_url`, `checked_on` | `duration_months` | `university_slug` + `name`: bor bo'lsa yangilanadi, yo'q bo'lsa yaratiladi |
| `/admin/costs/import` | `university_slug`, `academic_year`, `tuition_minor`, `currency`, `source_url`, `checked_on` | `living_minor` | faylda bor har bir universitetning narxlari fayldagisi bilan almashtiriladi |
| `/admin/scholarships/import` | `university_slug`, `name`, `kind`, `source_url`, `checked_on` | `amount_<til>`, `eligibility_<til>` | universitet grantlari almashtiriladi |
| `/admin/requirements/import` | `university_slug`, `program_name`, `intake_term`, `intake_year`, `source_url`, `checked_on` | `ielts_min`, `toefl_min`, `duolingo_min`, `sat_policy`, `sat_mid_range_low`, `sat_mid_range_high`, `minimum_gpa`, `gpa_scale`, `documents`, `essays_count`, `interview_required` | dastur talablari almashtiriladi |
| `/admin/deadlines/import` | `university_slug`, `program_name`, `round`, `due_date`, `source_url`, `checked_on` | — | dastur muddatlari almashtiriladi |

- **Format.** CSV, UTF-8 (BOM bo'lsa ham bo'ladi), birinchi qator sarlavha. Ajratuvchi vergul yoki nuqtali vergul — avtomatik aniqlanadi (Excel ba'zi tillarda `;` bilan saqlaydi). Ichida vergul bor qiymat qo'shtirnoqqa olinadi. Fayl ko'pi bilan 5 MB va 5 000 qator.
- **Qiymatlar.** Sana `YYYY-MM-DD`; sonlarda nuqta (`6.5`), vergul qabul qilinmaydi, shunda `6,5` jimgina `65` bo'lib qolmaydi; ha/yo'q `true`/`false`; enum'lar nomi bilan, katta-kichik harf farqi yo'q (`Bachelor`, `Fall`, `Required`, `NeedBased`); pul eng kichik birlikda (`tuition_minor`); `documents` nuqtali vergul bilan (`Transcript; CV`). Tarjima ustunlari `description_en`, `description_uz` ko'rinishida; yangi til uchun ustun qo'shish kifoya.
- **Hammasi yoki hech narsa.** Avval butun fayl tekshiriladi: sarlavha (yo'q yoki noma'lum ustun), har bir qiymat, qo'lda kiritishdagi bilan bir xil validator qoidalari, fayl ichidagi takrorlar va bazada universitet/dastur borligi. Bitta xato bo'lsa ham hech narsa saqlanmaydi va `400` javobida hamma xatolar `rows[5].currency` yoki `columns.rank` ko'rinishida keladi. Qator raqami Excel'dagi qator raqami (sarlavha 1-qator). Xato bo'lmasa hamma o'zgarish bitta tranzaksiyada saqlanadi va javobda `rows`, `created`, `updated` (ichki ro'yxatlar uchun — ro'yxati almashtirilgan universitet yoki dasturlar soni) keladi.
- **Nega validator handler ichida.** Boshqa buyruqlarda validator pipeline'da ishlaydi. Import buyrug'i esa faylni (`Stream`) olib keladi va qatorlar faqat fayl o'qilgandan keyin paydo bo'ladi, shuning uchun handler har bir qatorni o'sha validator'lar (`UniversityDetailsValidator`, `ReplaceUniversityCostsCommandValidator` va boshqalar) bilan tekshiradi. CSV'ni `ICsvReader` o'qiydi, uning CsvHelper'dagi amalga oshirilishi Infrastructure'da.

## Kurslar va guruhlar (Academy)

Academy 4Prep o'qitadigan kurslarni (Foundation Math, General English, SAT, IELTS va keyin qo'shiladiganlar), ularning guruhlarini va guruhlarga yozilishni saqlaydi. Ommaviy `/courses` sahifasi shu ma'lumotdan quriladi. Davomat, mock ballar va placement test keyingi bosqichda qo'shiladi.

**Tuzilishi.**

| Aggregate | Ichidagilar | Nima |
|---|---|---|
| `Course` | — | kurs: manzil nomi (`slug`, masalan `sat`), uch tilda nomi va tavsifi |
| `ClassGroup` | `ScheduleSlot`, `Enrolment` | kursning bitta guruhi: kodi, online yoki ofisda, boshlanish sanasi, necha hafta, o'rinlar soni, o'qituvchi, holati; haftalik dars jadvali; yozilishlar va navbat |

**Endpoint'lar.**

- Ochiq, login talab qilmaydi, `RateLimitPolicies.PublicRead`:
  - `GET /courses` — barcha kurslar va har birida nechta ochiq guruh borligi (`openGroupCount`).
  - `GET /courses/{slug}` — bitta kurs.
  - `GET /class-groups?course=&mode=&first=&rows=&sortField=&sortOrder=` — faqat `Open` holatidagi guruhlar, sahifada ko'pi bilan 100 ta (standart 20), standart saralash `starts_on`. `course` — kurs slug'i, `mode` — `1` online, `2` ofis.
  - `GET /class-groups/{id}` — bitta ochiq guruh (yozilish sahifasi uchun). Ochiq bo'lmagan guruh `404` qaytaradi.
- `mentor_admin` va `super_admin` (`Policies.CourseAdmin`):
  - `POST /admin/courses`, `PUT` / `DELETE /admin/courses/{id}`. Guruhi bor kursni o'chirib bo'lmaydi (`409 Course.HasClassGroups`).
  - `GET /admin/class-groups?course=&mode=&status=&teacherUserId=&search=` — barcha holatdagi guruhlar.
  - `POST /admin/courses/{courseId}/class-groups`, `PUT` / `DELETE /admin/class-groups/{id}`.
  - `PUT /admin/class-groups/{id}/status` `{ "status" }`.
  - `PUT /admin/class-groups/{id}/teacher` `{ "teacherUserId", "teacherName" }`, `DELETE /admin/class-groups/{id}/teacher`.

**Qoidalar va qarorlar.**

- **Kim boshqaradi.** Kurs va guruhlarni `content_admin` emas, `mentor_admin` va `super_admin` boshqaradi: `content_admin` faqat yangiliklar va media bilan ishlaydi. Buning uchun alohida `Policies.CourseAdmin` bor. Buyurtmachi mentorlarga ham ruxsat berishni so'rasa, `AuthenticationExtensions` dagi shu policy'ga `Roles.Mentor` qo'shiladi.
- **Guruh holati.** `Draft` (1) → `Open` (2) → `Closed` (3), istalgan holatdan → `Cancelled` (4); `Closed` guruhni qayta `Open` qilish mumkin. Boshqa o'tishlar `409 ClassGroup.InvalidStatusChange`. Yangi guruh `Draft` bo'lib yaratiladi, mehmonlarga faqat `Open` guruh ko'rinadi. Faqat `Draft` guruhni o'chirish mumkin (`ClassGroup.NotDraft`), e'lon qilingani bekor qilinadi. Bekor qilingan guruhni o'zgartirib bo'lmaydi.
- **Jadval va vaqt zonasi.** Haftalik jadvalda har bir dars kun (`0` yakshanba ... `6` shanba), boshlanish vaqti (`"18:00:00"`) va davomiylik (15–240 daqiqa, o'sha kuni tugaydi) bilan saqlanadi. Darslar ustma-ust tushmaydi, haftasiga ko'pi bilan 14 ta. Vaqt guruhning o'z IANA vaqt zonasida (`timeZone`, masalan `Asia/Tashkent` yoki `America/New_York`) yoziladi, frontend uni foydalanuvchi zonasiga o'girib ko'rsatadi. Shunda yozgi vaqtga o'tilganda dars soati guruh shahrida siljimaydi.
- **Ofis kodga yozilmagan.** Ofisdagi guruhda davlat (ISO kodi), shahar va manzil majburiy; spec'dagi "Toshkent ofisi" oddiy ma'lumot sifatida kiritiladi. Online guruhda manzil saqlanmaydi.
- **O'qituvchi.** Faqat `teacher` rolidagi foydalanuvchi biriktiriladi: rol `FourPrep.Modules.Academy.Auth` adapteri orqali tekshiriladi (`ClassGroup.NotATeacher`). Auth hisoblarida ism-familiya yo'q, shuning uchun kartada ko'rinadigan ismni (`teacherName`) admin biriktirish paytida yozadi. Content'da xodimlarning ochiq profillari paydo bo'lgach, ism o'sha yerdan olinadi. Xodim hisobi o'chirilsa, guruhda uning id'si va ismi qolib ketadi; buni event mexanizmi qo'shilganda tozalaymiz.
- **Narx hozircha yo'q.** Spec guruhga narx qo'yadi, lekin narx va to'lov shartnoma (Agreement) bilan birga qilinadi.
- **Guruh kodi** katta harflarga o'tkaziladi va butun tizimda unique (`SAT-2026-OCT`, `ClassGroup.CodeTaken`). Kurs slug'i ham unique (`Course.SlugTaken`).
- **Ko'p tilli matn.** Kurs nomi va tavsifi, guruhning kirish darajasi (`entryLevel`) Catalogue'dagidek `{ "en", "uz", "ru" }` ko'rinishida `jsonb` ustunda saqlanadi. Kurs nomining inglizchasi majburiy (`Course.NameRequired`).

### Yozilish (enrolment) va navbat

O'quvchi o'zini, ota-ona esa hisobiga bog'langan farzandini `Open` guruhga yozadi. Bo'sh o'rin bo'lsa, yozilish `PendingPayment` (2) bo'ladi va o'rin 24 soat band turadi (`paymentDueOnUtc`). Guruh to'lgan bo'lsa, o'quvchi `Waitlisted` (1) bo'lib navbatga turadi. Payment moduli bo'lmagani uchun to'lov olingach admin yozilishni qo'lda `Active` (3) qiladi. Keyin buni to'lov webhook'i qiladi.

**Endpoint'lar.**

- O'quvchi va ota-ona (`Policies.StudentOrParent`):
  - `POST /class-groups/{id}/enrolments` `{ "studentUserId" }` — o'quvchi o'z id'sini, ota-ona farzandining `userId` sini (`GET /parents/me/students` dan) yuboradi. Javob: `{ id, status, paymentDueOnUtc }`.
  - `GET /enrolments?studentUserId=` — o'quvchining barcha yozilishlari, kurs va guruh ma'lumoti bilan; navbatdagilarda `waitlistPosition` (1 dan boshlanadi). Oilaga kim yozgani id bilan emas, `enrolledByMe` bilan ko'rsatiladi: bir nechta ota-ona bir-birining hisob id'sini ko'rmaydi.
  - `DELETE /enrolments/{id}` — navbatdagi yoki to'lov kutilayotgan yozilishni bekor qiladi.
- `mentor_admin` va `super_admin` (`Policies.CourseAdmin`):
  - `GET /admin/class-groups/{id}/enrolments` — guruhning barcha yozilishlari (holat, keyin yozilgan vaqt bo'yicha).
  - `POST /admin/enrolments/{id}/activate` — to'lov olingach o'rinni tasdiqlaydi.
  - `DELETE /admin/enrolments/{id}` — istalgan joriy yozilishni (faolini ham) olib tashlaydi.

**Qoidalar va qarorlar.**

- **Kim kimni yozadi.** Bu o'quvchining o'zi yoki unga bog'langan ota-onami, buni Student moduli `FourPrep.Modules.Academy.Student` adapteri orqali (`HasFamilyAccessQuery`) aytadi. Boshqa odam `403 Enrolment.NotYourStudent` oladi. Bekor qilishda esa begona yozilish `404 Enrolment.NotFound` qaytaradi, shunda id orqali boshqa oilaning yozilishi borligi bilinmaydi. Kim tugmani bosgani `enrolledByUserId` da saqlanadi.
- **To'lanmagan yozilishlar chegarasi.** Bitta o'quvchida bir vaqtda ko'pi bilan 3 ta to'lanmagan yozilish (navbat yoki to'lov kutilmoqda) bo'ladi, hamma guruhlar bo'yicha (`Enrolment.TooManyUnpaid`). Faol yozilishlar hisobga kirmaydi. Shunday qilib bir hisob to'lamasdan ko'p guruhni band qila olmaydi.
- **O'rinlar.** `PendingPayment` va `Active` o'rin egallaydi, ommaviy guruh kartasida `seatsLeft` shu bilan hisoblanadi. Bir o'quvchi bitta guruhda bir vaqtda faqat bitta joriy yozilishga ega (`Enrolment.AlreadyEnrolled`). Bekor qilgandan keyin qayta yozilishi mumkin. Guruhni tahrirlashda `capacity` o'rin egallaganlar sonidan kam bo'lolmaydi (`ClassGroup.CapacityBelowSeatsTaken`).
- **Navbat avtomatik.** O'rin bo'shasa (bekor qilish, admin olib tashlashi, muddat o'tishi yoki `capacity` oshishi), navbatdagi birinchi odam o'zi `PendingPayment` ga o'tadi va unga 24 soat beriladi. Navbat tartibi yozilgan vaqt bo'yicha. Unga xabar yuborish Notification moduli bilan qo'shiladi; hozircha frontend `GET /enrolments` dan ko'radi.
- **Muddat.** Hangfire har 5 daqiqada `academy-expire-unpaid-enrolments` job'ini ishga tushiradi. U muddati o'tgan `PendingPayment` yozilishlarni `Dropped` (5, `dropReason: 3` — to'lanmadi) qiladi va bo'shagan o'rinlarni navbatga beradi. Shuning uchun o'rin muddatdan keyin 5 daqiqagacha band turishi mumkin. Job hamma guruhni bitta saqlashda yozadi: shu paytda biror guruh boshqa so'rov bilan o'zgarsa, butun to'plam keyingi ishga tushishda qayta urinadi.
- **Bekor qilish.** Oila faqat navbatdagi yoki to'lov kutilayotgan yozilishni bekor qiladi (`dropReason: 1`). Faol, ya'ni to'langan yozilishni faqat admin olib tashlaydi (`dropReason: 2`), chunki pul qaytarilmaydi (`Enrolment.CannotCancel`). Yozilishlar o'chirilmaydi, tarix uchun `Dropped` bo'lib qoladi.
- **Bir vaqtdagi so'rovlar.** Yozilishlar `ClassGroup` aggregate'ining ichida, guruh qatorida esa `xmin` versiyasi bor. Oxirgi o'ringa ikki kishi bir vaqtda yozilsa, ikkinchisi `409 Conflict.DuplicateKey` oladi va qayta urinadi; o'rinlar sonidan oshib ketmaydi.
- **Guruh bekor qilinsa,** undagi barcha joriy yozilishlar (navbat, to'lov kutilayotgan va faol) `Dropped` bo'ladi (`dropReason: 4` — guruh bekor qilindi). To'lagan oilani admin boshqa guruhga qo'lda o'tkazadi, pul qaytarilmaydi.
- **Hali yo'q:** `Completed` (4) holati kurs tugashi bilan birga qo'shiladi. O'quvchi hisobi o'chirilganda uning yozilishlari Academy'da qoladi; event mexanizmi bilan tozalanadi. Spec'dagi to'lov sahifasi ochiq turgan 30 daqiqalik band Payment bilan keladi. Admin ro'yxatida o'quvchi ismi yo'q, faqat `studentUserId`: ismni frontend Student'dan (`GET /students`) oladi.

## Yangiliklar (Content)

Content sayt uchun yangiliklarni saqlaydi. Natijalar (results), hamkorlar, About va xodimlarning ochiq profillari keyingi bosqichda shu modulga qo'shiladi. Yangilikni `content_admin` (va `super_admin`) staff panelda yozadi va e'lon qiladi. Mehmon esa ularni login qilmasdan o'qiydi.

**Tuzilishi.**

| Aggregate | Nima |
|---|---|
| `NewsPost` | yangilik: manzil nomi (`slug`), bir yoki bir nechta tildagi sarlavha va matn, teg, muqova rasmi, holati (`Draft` yoki `Published`) |

**Endpoint'lar.**

- Ochiq, login talab qilmaydi, `RateLimitPolicies.PublicRead`:
  - `GET /news?locale=&tag=&first=&rows=&sortField=&sortOrder=` — faqat e'lon qilingan yangiliklar, kartochka uchun matnsiz. Sahifada ko'pi bilan 100 ta (standart 20), standart saralash `published_on_utc` bo'yicha, eng yangisi birinchi. `locale` berilsa, faqat shu tilda sarlavhasi bor yangiliklar keladi.
  - `GET /news/{slug}` — bitta e'lon qilingan yangilik, matni bilan. Qoralama `404` qaytaradi.
- `content_admin` va `super_admin` (`Policies.ContentAdmin`):
  - `GET /admin/news?status=&tag=&locale=&search=` — barcha yangiliklar (qoralamalar ham), standart saralash `updated_on_utc` bo'yicha. `search` manzil nomidan va har qaysi tildagi sarlavhadan qidiradi.
  - `GET /admin/news/{id}` — bitta yangilik, `coverFileId` bilan.
  - `POST /admin/news` `{ "slug", "title", "body", "tag" }`, `PUT` / `DELETE /admin/news/{id}`.
  - `POST /admin/news/{id}/publish`, `POST /admin/news/{id}/unpublish`.
  - `PUT /admin/news/{id}/cover` `{ "fileId" }`, `DELETE /admin/news/{id}/cover`.

**Qoidalar va qarorlar.**

- **Tillar.** Sarlavha kamida bitta tilda bo'lishi kerak (`NewsPost.TitleRequired`), inglizchasi shart emas: faqat o'zbekcha yoki faqat ruscha yangilik ham bo'ladi. Sarlavha va matn bir xil tillarda yoziladi (`NewsPost.TextLanguagesMismatch`): sarlavhasi bor tilda matn ham bo'lishi kerak, matni bor tilda esa sarlavha. Shuning uchun Catalogue va Academy'dagi "inglizchasi majburiy" qoidasi (`LocalizedText`) bu yerda ishlatilmaydi, uning o'rniga Common'dagi `LocalizedTextInAnyLanguage` bor. Frontend ro'yxatni foydalanuvchi tilida so'raydi (`?locale=uz`) yoki hammasini olib, boshqa tildagi yangilikni belgi bilan ko'rsatadi.
- **Matn Markdown'da.** `body` ning har bir tili Markdown (sarlavha, ro'yxat, havola, qalin matn), har biri 20 000 belgigacha. HTML saqlanmaydi. Frontend Markdown'ni xom HTML'siz render qiladi, shunda yangilik orqali sahifaga skript tushmaydi.
- **Manzil nomi va teg.** `slug` kichik lotin harflari, raqamlar va chiziqchadan iborat, 150 belgigacha va unique (`NewsPost.SlugTaken`). Uni e'lon qilingandan keyin ham o'zgartirish mumkin, lekin eski havola ishlamay qoladi. Teg ixtiyoriy va xuddi shu ko'rinishdagi kalit (`admissions`, `courses`). Uning uch tildagi nomini frontend o'zi chiqaradi, bazada matn sifatida saqlanmaydi.
- **Holat.** Yangi yangilik `Draft` bo'ladi, mehmonga faqat `Published` ko'rinadi. `publishedOnUtc` birinchi e'londa qo'yiladi va qayta e'lon qilinganda o'zgarmaydi: xatoni tuzatish uchun olib qo'yilib, qayta chiqarilgan yangilik ro'yxat boshiga sakramaydi. E'lon qilingan yangilikni tahrirlash mumkin, u e'londa qoladi. Ikki marta e'lon qilish `409 NewsPost.AlreadyPublished`, qoralamani olib qo'yish `409 NewsPost.NotPublished` qaytaradi. Yangilikni istalgan holatda o'chirish mumkin.
- **Muqova rasmi.** Admin rasmni sayt rasmi sifatida yuklaydi (`POST /files/upload-url` `{ ..., "purpose": 2 }` → `PUT` → `POST /files/{fileId}/complete`, pastda "Fayllar (Storage)"), keyin `PUT /admin/news/{id}/cover` bilan biriktiradi. Content fayl haqida Storage'dan `FourPrep.Modules.Content.Storage` adapteri orqali so'raydi.
  - Faqat o'zi yuklagan fayl biriktiriladi, boshqasiniki `404 NewsPost.CoverNotFound` qaytaradi. Aks holda id'ni bilgan admin o'quvchining pasportini ochiq qilib qo'yishi mumkin edi.
  - Faqat JPG va PNG (`NewsPost.CoverNotAnImage`), chunki brauzerlar HEIC'ni ko'rsatmaydi. Bitta rasm faqat bitta yangilikning muqovasi bo'ladi (`NewsPost.CoverInUse`).
  - Virus tekshiruvi tugamagan bo'lsa, Storage'ning `409 StoredFile.NotScanned` xatosi qaytadi va frontend bir necha soniyadan keyin qayta yuboradi. Hujjat sifatida (`purpose` siz yoki `1`) yuklangan fayl `409 StoredFile.NotPublicMedia` qaytaradi.
  - Biriktirilganda Storage faylni ochiq bucket'ga nusxalaydi va yangilik uning doimiy URL'ini (`coverUrl`) saqlaydi. Muqova almashtirilsa, olib tashlansa yoki yangilik o'chirilsa, ochiq nusxa yangilik saqlanishidan oldin o'chiriladi: ombor xato bersa, hech narsa saqlanmaydi va admin qayta urinadi, rasm esa ochiq qolib ketmaydi. Asl fayl yopiq bucket'da qoladi.
  - Rasm ochiq qilingandan keyin yangilikni saqlab bo'lmasa (baza ishlamay qoldi yoki boshqa so'rov shu rasmni oldinroq oldi), yangi rasm ochiq bucket'dan qaytarib olinadi, agar uni boshqa yangilik ishlatmayotgan bo'lsa. Buni tekshirib bo'lmasa (baza javob bermasa), rasm baribir olinadi: begona yangilikning rasmi bir muddat ochilmay qolishi, rasm ochiq qolib ketishidan yaxshiroq. Qaytarib olish o'zi ham muvaffaqiyatsiz bo'lsa, bu log'ga yoziladi va so'rov asl xato bilan tugaydi. Eski muqova esa saqlashdan oldin o'chirilgan bo'ladi: yangilik eski `coverUrl` bilan qoladi, rasm ochilmaydi va admin muqovani qayta qo'yadi. Noo'rin rasmni almashtirganda u ochiq qolib ketmasligi uchun tartib shunday tanlangan.
- **Qoralama muqovasi.** Rasm biriktirilishi bilan ochiq bucket'ga chiqadi, yangilik e'lon qilinishini kutmaydi: admin qoralamani rasmi bilan ko'ra olishi uchun. URL tasodifiy (`public/<uuid>`), bucket ro'yxatini esa imzosiz olib bo'lmaydi, shuning uchun uni topib bo'lmaydi.
- **Javoblar.** Ochiq javobda `coverFileId` yo'q, faqat `coverUrl` bor. Muallif hozircha saqlanmaydi: kim nima qilganini Audit moduli yozadi.

## Uchrashuvlar (Meeting)

Meeting bepul 45 daqiqalik konsultatsiyani (o'quvchi yo'lining 1-bosqichi) bron qilishni boshqaradi. Uchrashuvning o'zi platformada emas, Zoom yoki Google Meet'da o'tadi: spec'dagi LiveKit video xonasi o'rniga har bir xodimning o'z doimiy havolasi ishlatiladi. Hozircha faqat konsultatsiya bor. Mentor sessiyasi, mock interview va ota-ona uchrashuvi keyin shu tuzilmaga qo'shiladi.

**Tuzilishi.**

| Aggregate | Ichidagilar | Nima |
|---|---|---|
| `StaffSchedule` | `AvailabilityWindow` | xodimning uchrashuv jadvali: ko'rinadigan ismi, vaqt zonasi, doimiy Zoom/Meet havolasi, ofis manzili va haftalik bo'sh vaqtlari (kun, boshlanish, tugash, format) |
| `Meeting` | — | bitta uchrashuv: turi, formati (`1` online, `2` ofis, `3` telefon), holati (`1` rejada, `2` bekor, `3` o'tkazildi, `4` kelmadi), o'quvchi, xodim, vaqti, havola yoki manzil |

**Endpoint'lar.**

- O'quvchi va ota-ona (`Policies.StudentOrParent`):
  - `GET /meetings/slots?studentUserId=&mode=&fromUtc=&toUtc=` — bo'sh vaqtlar: `{ staffUserId, staffName, startsOnUtc, durationMinutes, modes }`. Standart davr — 12 soatdan keyingi 14 kun, ko'pi bilan 31 kun.
  - `POST /meetings` `{ "studentUserId", "staffUserId", "startsOnUtc", "mode", "phoneNumber" }` — bron qiladi, javobda uchrashuv id'si. Telefon formatida `phoneNumber` (E.164) majburiy.
  - `GET /meetings?studentUserId=` — o'quvchining hamma uchrashuvlari.
  - `DELETE /meetings/{id}` — uchrashuv boshlanguncha bekor qiladi.
- Xodim (`Policies.Mentor`: `mentor`, `mentor_admin`, `super_admin`):
  - `GET /meetings/schedules/me`, `PUT /meetings/schedules/me` `{ "displayName", "timeZone", "meetingUrl", "officeCountryCode", "officeCity", "officeAddress", "windows": [{ "day", "startTime", "endTime", "mode" }] }`.
  - `GET /meetings/staff/me?status=&mode=&fromUtc=&toUtc=&search=` — xodimning o'z uchrashuvlari, standart saralash `starts_on_utc` bo'yicha.
  - `POST /meetings/{id}/cancel` `{ "reason" }`, `PUT /meetings/{id}/attendance` `{ "status": 3 | 4 }`, `PUT /meetings/{id}/notes` `{ "notes" }`, `PUT /meetings/{id}/link` `{ "meetingUrl" }` — faqat o'z uchrashuvi uchun. Begonasi `404`.
- `mentor_admin` va `super_admin` (`Policies.MentorAdmin`):
  - `GET /admin/meetings?status=&mode=&staffUserId=&studentUserId=&fromUtc=&toUtc=&search=`.
  - `POST /admin/meetings/{id}/cancel` `{ "reason" }` — istalgan uchrashuvni bekor qiladi.

**Qoidalar va qarorlar.**

- **Havola tashqarida.** Xodim jadvalida bitta doimiy havola saqlaydi (Zoom Personal Meeting Room yoki doimiy Google Meet). Online bron shu havolani o'zi bilan oladi. Xodim kerak bo'lsa uni bitta uchrashuv uchun almashtiradi (`PUT /meetings/{id}/link`). Havola faqat `https://` va faqat Zoom (`zoom.us` va uning subdomenlari) yoki Google Meet (`meet.google.com`) bo'ladi. Login-parol qismi (`https://zoom.us@...`), boshqa port, IP yoki boshqa domen rad etiladi (`StaffSchedule.InvalidMeetingUrl`). Bu havolani bolalar va ota-onalar bosadi, shuning uchun buzilgan xodim hisobi uni fishing sahifasiga almashtira olmasligi kerak. Teams yoki boshqa xizmat kerak bo'lsa, domen `MeetingLinks` ga qo'shiladi. Zoom yoki Google API bilan integratsiya yo'q: 4Prep'ning Zoom yoki Google akkaunti, OAuth va har uchrashuvga alohida xona kerak bo'lsa, `MeetingUrl` ni bron paytida API yaratadigan qilib qo'shiladi.
- **Kim bilan.** O'quvchiga mentor biriktirilgan bo'lsa, faqat o'sha mentorning vaqtlari ko'rinadi va boshqa xodim bilan bron `409 Meeting.MentorOnly` qaytaradi. Mentor bo'lmasa, jadvali bor istalgan xodimning vaqtlari ko'rinadi. Jadval tuzgan xodim konsultant hisoblanadi, alohida rol yo'q. Bron paytida xodim hali `mentor`, `mentor_admin` yoki `super_admin` ekani `FourPrep.Modules.Meeting.Auth` adapteri orqali tekshiriladi: roli olib tashlangan xodimga o'quvchi ma'lumoti bormaydi (`Meeting.SlotNotAvailable`). Bo'sh vaqtlar ro'yxati rolni tekshirmaydi (har bir xodim uchun Keycloak'ka so'rov bo'lardi), shuning uchun bunday xodimning jadvali event mexanizmi qo'shilguncha ro'yxatda ko'rinib turishi mumkin. Mentorni Student `FourPrep.Modules.Meeting.Student` adapteri orqali (`GetStudentContactQuery`) aytadi, oila ruxsatini esa `HasFamilyAccessQuery` tekshiradi.
- **Vaqt zonasi.** So'rovdagi vaqtlar (`startsOnUtc`, `fromUtc`, `toUtc`) offset bilan kelsa shu onga o'giriladi, offset'siz kelsa UTC deb o'qiladi. Bo'sh vaqtlar xodimning o'z IANA zonasida yoziladi (`America/New_York`, `Asia/Tashkent`), uchrashuv esa UTC'da saqlanadi. Frontend vaqtni foydalanuvchi zonasida ko'rsatadi. Yozgi vaqtga o'tilganda slotlar xodim shahrida siljimaydi. Mavjud bo'lmagan mahalliy vaqt (soat oldinga surilgan tun) tashlab ketiladi.
- **Slotlar.** Har bir bo'sh vaqt ichida 30 daqiqalik qadam bilan boshlanadi va 45 daqiqalik uchrashuv oyna ichiga sig'ishi kerak (09:00–11:00 → 09:00, 09:30, 10:00). Xodimning rejadagi uchrashuvi bilan ustma-ust tushadigan slot ko'rsatilmaydi. Bir vaqtda bir nechta format ochiq bo'lsa, slot ularni `modes` da birga beradi. Bir kunda bir xil formatdagi oynalar ustma-ust tushmaydi (`AvailabilityWindow.Overlapping`), har xil formatdagilari tushishi mumkin.
- **Bron qoidalari.** Kamida 12 soat oldin va ko'pi bilan 30 kun oldinga. Vaqt jadvalda aynan shu format bilan taklif qilinishi kerak, aks holda `409 Meeting.SlotNotAvailable`. Bitta o'quvchida bir vaqtda bitta rejadagi konsultatsiya bo'ladi (`Meeting.AlreadyBooked`). Bitta foydalanuvchi (ota-ona bir nechta farzandi uchun) bir vaqtda ko'pi bilan 3 ta rejadagi konsultatsiya bron qiladi (`Meeting.TooManyBooked`). Bekor qilingandan keyin qayta bron qilish mumkin.
- **Suiiste'moldan himoya.** Ro'yxatdan o'tish ochiq, shuning uchun bir kishi ko'p hisob ochib mentorlarning vaqtini band qilib qo'yishi mumkin. `POST /meetings` va `DELETE /meetings/{id}` ga bitta IP'dan soatiga 20 ta so'rov ruxsat etiladi (`RateLimitPolicies.Booking`, oshsa `429`), `GET /meetings/slots` ga esa `PublicRead` (daqiqasiga 120). Tasdiqlangan email talab qilish (`Policies.VerifiedEmail`) hali yoqilmagan.
- **Bir vaqtdagi bronlar.** Xodim jadvali qatorida `xmin` versiyasi bor va har bir bron uni yangilaydi. Bir xodimga ikki kishi bir vaqtda bron qilsa, ikkinchisi `409 Conflict.DuplicateKey` oladi va bo'sh vaqtlarni qayta so'raydi. Bazada ham bir xodim va bir boshlanish vaqtiga, bir o'quvchi va bitta konsultatsiyaga rejadagi uchrashuv uchun qisman unique indeks bor.
- **Nusxa qoladi.** Uchrashuv bron paytidagi xodim ismi, havola va ofis manzilini o'zida saqlaydi. Xodim jadvalini keyin o'zgartirsa, oldin bron qilingan uchrashuvlar o'zgarmaydi.
- **Bekor qilish.** Oila uchrashuvni boshlanguncha bekor qiladi (`Meeting.AlreadyStarted`). Xodim o'z uchrashuvini sabab bilan bekor qiladi, `mentor_admin` esa istalganini, boshlangandan keyin ham (masalan, o'tkazilmay qolgan uchrashuvni yopish uchun). Sabab oilaga ko'rinadi, shuning uchun u oilaga yozilgan matn bo'lishi kerak. Uchrashuvlar o'chirilmaydi, `Cancelled` bo'lib qoladi.
- **Qatnashuv.** Uchrashuv boshlangandan keyin xodim uni `Held` (3) yoki `NoShow` (4) deb belgilaydi va keyin tuzatishi mumkin. Xodimning izohi (`staffNotes`) faqat xodimlarga ko'rinadi.
- **Kim nimani ko'radi.** Oila javobida xodimning id'si, izohi va kim bron qilgani yo'q, faqat `bookedByMe`. Havola faqat rejadagi uchrashuvda ko'rsatiladi. Xodim uchrashuvda o'quvchining ko'rinadigan ismini (ism-familiya, bo'lmasa username) va telefon uchrashuvida telefonini ko'radi. Telefon raqami uchrashuv bekor qilinishi yoki qatnashuv belgilanishi bilan o'chiriladi. Bu ism bron paytida Meeting'ga nusxalanadi, chunki mentori bo'lmagan konsultant Student profilini o'qiy olmaydi. Oila shu xodim bilan bron qilib, ismini unga o'zi beradi; xodim profilning qolgan qismini (tug'ilgan sana, maktab, davlat) ko'rmaydi.
- **Hali yo'q:** natija formasi (yo'nalish, davlatlar, yo'l, keyingi qadam) va o'quvchi bosqichini o'zgartirish (Student/Admission bilan), 24 va 1 soat oldingi eslatmalar (Notification bilan), boshqa uchrashuv turlari, ko'chirish (hozir bekor qilib qayta bron qilinadi). O'quvchi hisobi o'chirilganda uning uchrashuvlari va nusxalangan ismi Meeting'da qoladi. Ular event mexanizmi bilan tozalanadi.

## Academy arizasi (Admission)

Academy arizasi Lead'ni Applicant qiladi: o'quvchi 4Prep xizmatlariga qiziqishini, hozirgi ballarini va qanday bog'lanish qulayligini yuboradi, `mentor_admin` uni ko'rib chiqadi. Admission'ning qolgan qismi (universitetlarga arizalar, portal akkauntlari, essay, tavsiyanomalar, offerlar, viza) keyin qo'shiladi.

**Tuzilishi.**

| Aggregate | Ichidagilar | Nima |
|---|---|---|
| `AcademyApplication` | `ReportedScore` | bitta ariza: qiziqishlar (`1` qabul paketi, `2` test-prep, `3` kurslar), o'quvchi o'zi kiritgan ballar, aloqa kanallari (`1` email, `2` telefon, `3` Telegram), telefon, Telegram username, uchrashuv tili, erkin savol, ota-ona roziligi, holati (`1` yuborilgan, `2` qabul qilingan, `3` rad etilgan, `4` qaytarib olingan) |

**Endpoint'lar.**

- O'quvchi (`Policies.Student`):
  - `POST /academy-applications` `{ "interests": [1, 2], "scores": [{ "exam", "score" }], "contactChannels": [1, 3], "phoneNumber", "telegramUsername", "meetingLanguage": "uz", "question", "parentConsentConfirmed" }` — arizani yuboradi, javobda ariza id'si.
  - `POST /academy-applications/{id}/withdraw` — yuborilgan (hali ko'rib chiqilmagan) arizani qaytarib oladi. Begonasi `404`.
- O'quvchi va ota-ona (`Policies.StudentOrParent`):
  - `GET /academy-applications?studentUserId=` — o'quvchining hamma arizalari, yangisi birinchi. Faqat o'quvchining o'zi va bog'langan ota-onasi (`403 AcademyApplication.NotYourStudent`).
- `mentor_admin` va `super_admin` (`Policies.MentorAdmin`):
  - `GET /admin/academy-applications?status=&interest=&studentUserId=&search=` — pipeline ro'yxati, standart saralash `submitted_on_utc` bo'yicha kamayish tartibida.
  - `GET /admin/academy-applications/{id}` — to'liq ariza.
  - `POST /admin/academy-applications/{id}/accept`, `POST /admin/academy-applications/{id}/decline` `{ "reason" }`.

**Qoidalar va qarorlar.**

- **Bir marta yuboriladi.** Frontend 4 qadamni o'zida ushlaydi va oxirida bitta `POST` yuboradi, serverda qoralama yo'q. O'quvchining ism-familiyasi, tug'ilgan sanasi va maktabi Student profilida turadi (`PUT /students/me`), arizada takrorlanmaydi. Spec'dagi "yo'nalishlar" (davlatlar, daraja, intake) qadami hozircha arizada yo'q.
- **Ballar o'quvchining so'zi.** Ballar arizada nusxa sifatida saqlanadi va ular rasmiy emas: English gate uchun rasmiy ballni keyin mentor Student'ga kiritadi. Har imtihon bir martadan, ko'pi bilan 4 ta, har biri o'sha imtihon shkalasida (`ExamScores`).
- **Bitta ochiq ariza.** O'quvchida bir vaqtda bitta yuborilgan yoki qabul qilingan ariza bo'ladi (`409 AcademyApplication.AlreadyApplied`, bazada qisman unique indeks ham bor). Rad etilgan yoki qaytarib olingandan keyin qayta yuborish mumkin. `POST /academy-applications` ga bitta IP'dan soatiga 20 ta so'rov ruxsat etiladi (`RateLimitPolicies.Booking`, oshsa `429`).
- **Holatlar.** Faqat yuborilgan ariza qabul qilinadi, rad etiladi yoki qaytarib olinadi, boshqasi `409 AcademyApplication.NotSubmitted`. Kim va qachon ko'rib chiqqani saqlanadi. Ariza qatorida `xmin` versiyasi bor: `mentor_admin` qabul qilayotgan paytda o'quvchi qaytarib olsa, ikkinchisi `409 Conflict.DuplicateKey` oladi. Rad etish sababi majburiy va oilaga ko'rinadi, shuning uchun u oilaga yozilgan matn bo'lishi kerak. Qabul qilish hozircha faqat holatni o'zgartiradi: mentor biriktirish Student'da (`PUT /students/{id}/mentor`), konsultatsiya Meeting'da qoladi.
- **Voyaga yetmaganlar.** 18 yoshga to'lmagan o'quvchi "ota-onam xabardor va rozi" ni belgilashi shart (`400 AcademyApplication.ParentConsentRequired`). Yosh Student profilidagi tug'ilgan sanadan o'quvchining o'z vaqt zonasidagi bugungi kun bo'yicha hisoblanadi (`FourPrep.Modules.Admission.Student` adapteri, `GetStudentContactQuery`). Tug'ilgan sana kiritilmagan bo'lsa, o'quvchi voyaga yetmagan deb hisoblanadi. Bog'langan ota-ona ariza vaqtida shart emas. 18 hamma davlatda ham voyaga yetish yoshi emas; boshqa chegara kerak bo'lsa, `StudentAge` dagi qiymat o'zgaradi.
- **Aloqa.** Kamida bitta kanal tanlanadi. Telefon tanlansa xalqaro formatdagi raqam (E.164), Telegram tanlansa username (5–32 belgi, harf bilan boshlanadi, `@` olib tashlanadi) majburiy. Tanlanmagan kanal ma'lumoti saqlanmaydi. Email Keycloak hisobidagi email'dan olinadi, arizada saqlanmaydi. Uchrashuv tili `en`, `uz` yoki `ru`.
- **Kim nimani ko'radi.** Oila javobida kim ko'rib chiqqani (`reviewedByUserId`) yo'q. O'quvchining ismi yuborish paytida arizaga nusxalanadi, `mentor_admin` ro'yxatni shu ism bo'yicha qidiradi.
- **Hali yo'q:** yo'nalishlar qadami, arizaga bildirishnoma (Notification bilan), qabul qilinganda o'quvchi bosqichini (stage) o'zgartirish, ota-onaning farzandi nomidan yuborishi. O'quvchi hisobi o'chirilganda uning arizalari Admission'da qoladi; ular event mexanizmi bilan tozalanadi.

## Yozishmalar (Chat)

Chat o'quvchi va oilasini 4Prep xodimlari bilan bog'laydi. Har o'quvchida ikkita thread bor: biri hozirgi mentori bilan, biri support bilan. O'quvchi ham, unga bog'langan har bir ota-ona ham ikkala thread'ni o'qiydi va yoza oladi. O'qituvchi thread'lari, fayl biriktirish va real vaqt (SignalR) keyin qo'shiladi.

**Tuzilishi.**

| Aggregate | Nima |
|---|---|
| `ChatThread` | bitta o'quvchining bitta thread'i: `kind` (`1` mentor, `2` support), o'quvchining nusxalangan ismi. Har o'quvchi va tur uchun bittadan (unique indeks) |
| `ChatMessage` | thread'dagi xabar: kim yozgani (`senderUserId`, nusxalangan `senderName`), tomoni (`1` oila, `2` xodim), matn, vaqti |
| `ChatReadMark` | foydalanuvchi thread'ni oxirgi marta qachon o'qigani. O'qilmagan xabarlar soni shundan hisoblanadi |

**Endpoint'lar.** `{kind}` — `Mentor` yoki `Support` (aynan shu yozilishda) yoki `1`, `2`. Boshqa yozilish (`support`) hozircha `500` qaytaradi: host binding xatolarini `400` ga aylantirmaydi.

- O'quvchi va ota-ona (`Policies.StudentOrParent`, faqat o'z hisobi yoki bog'langan farzand, boshqasi `403 ChatThread.NotYourStudent`):
  - `GET /chat/students/{studentUserId}/threads` — ikkala thread: oxirgi xabar (200 belgigacha), uni kim yozgani, vaqti, o'qilmaganlar soni, `canWrite`. Hali xabar bo'lmagan thread `id: null` bilan qaytadi.
  - `GET /chat/students/{studentUserId}/threads/{kind}/messages?before=&limit=` — xabarlar, yangisi birinchi. `limit` standart 50, 1–100 oralig'iga keltiriladi. Oldingi sahifa uchun `before` ga ro'yxatdagi eng eski xabar id'si beriladi.
  - `POST /chat/students/{studentUserId}/threads/{kind}/messages` `{ "body" }` — xabar yuboradi, javobda xabar id'si. Thread birinchi xabar bilan ochiladi.
  - `POST /chat/students/{studentUserId}/threads/{kind}/read` — thread'ni hozirgacha o'qildi deb belgilaydi.
- Mentor (`Policies.Mentor`, faqat o'zi biriktirilgan o'quvchining mentor thread'i):
  - `GET /staff/chat/threads?awaitingReply=&unreadOnly=&search=` — o'z o'quvchilarining mentor thread'lari, standart saralash `last_message_on_utc` bo'yicha kamayish tartibida.
  - `GET /staff/chat/students/{studentUserId}/messages`, `POST /staff/chat/students/{studentUserId}/messages`, `POST /staff/chat/students/{studentUserId}/read`.
- `mentor_admin` va `super_admin` (`Policies.MentorAdmin`, hamma thread'lar):
  - `GET /admin/chat/threads?kind=&awaitingReply=&unreadOnly=&search=` — support navbati va hamma mentor thread'lari.
  - `GET`, `POST /admin/chat/students/{studentUserId}/threads/{kind}/messages`, `POST /admin/chat/students/{studentUserId}/threads/{kind}/read`.

**Qoidalar va qarorlar.**

- **Kim kiradi.** Mentor thread'ini o'quvchi, uning ota-onalari, hozirgi mentori, `mentor_admin` va `super_admin` ko'radi. Support'ni mentor ko'rmaydi. Mentor almashtirilsa, eski mentor thread'ga darrov kira olmay qoladi, yangisi butun tarixni ko'radi. Tekshiruv har so'rovda `FourPrep.Modules.Chat.Student` adapteri orqali Student'dan olinadi (`HasFamilyAccessQuery`, `GetStudentContactQuery`). Mentor navbati uchun Student'ga `GetMenteeUserIdsQuery` qo'shildi.
- **Mentor hali yo'q.** Mentor biriktirilmagan o'quvchining mentor thread'iga hech kim yoza olmaydi (`409 ChatThread.NoMentor`), `canWrite: false`. Eski xabarlar o'qilaveradi. Mentor tayinlanguncha oila support'ga yozadi.
- **Xabar.** Faqat matn, bo'sh joylar kesilgandan keyin 1–4000 belgi (`400 ChatMessage.BodyRequired`). Xabar tahrirlanmaydi va o'chirilmaydi. Har foydalanuvchiga daqiqasiga 30 ta xabar ruxsat etiladi (`RateLimitPolicies.Messaging`, oshsa `429`).
- **Ismlar.** Keycloak hisobida ism yo'q, shuning uchun xabar yozilgan paytdagi ism nusxalanadi: o'quvchining o'zi uchun Student profilidagi ism, ota-ona va xodimlar uchun username. Xodim username'i staff panelga kirish login'i, shuning uchun oila javobida xodim xabarining `senderUserId` va `senderName` maydonlari `null` bo'ladi. Oila faqat `sender: 2` ni ko'radi va uni thread turiga qarab "Mentoringiz" yoki "4Prep support" deb ko'rsatadi (Meeting ham oiladan xodimni yashiradi). Har xabarda `fromMe` bor. Xodimlarning ochiq profillari (Content) paydo bo'lganda xodim ismi ko'rsatilishi mumkin.
- **Oila ichida maxfiylik yo'q.** O'quvchining ota-onasidan yashirin kanali yo'q: bog'langan har bir ota-ona ikkala thread'ning butun tarixini, jumladan bog'lanishdan oldingi xabarlarni ham ko'radi. Link code'ni olgan begona odam ham shuni ko'radi, shuning uchun link code faqat o'quvchiga ko'rsatiladi.
- **Matn xom saqlanadi.** Xabar va `lastMessagePreview` HTML sifatida emas, faqat matn sifatida chiqarilishi kerak (oila va staff paneli ikkalasida ham).
- **O'qilmaganlar.** O'qilmagan xabar — boshqa odam yozgan va foydalanuvchining oxirgi `read` belgisidan keyin kelgan xabar. Har kimning belgisi o'zida: o'quvchi o'qigani ota-ona uchun o'qilgan hisoblanmaydi, bitta `mentor_admin` o'qigani boshqa admin uchun ham. `awaitingReply` — oxirgi xabarni oila yozgan, xodim hali javob bermagan. Xodim navbatda `unreadOnly=true&rows=1` ni so'rasa, `totalCount` yon menyudagi son bo'ladi.
- **Real vaqt yo'q.** Frontend thread ro'yxatini va ochiq thread'ni so'rab turadi (polling). SignalR va "yangi xabar" bildirishnomasi Notification bilan qo'shiladi.
- **Hali yo'q:** o'qituvchi thread'lari (Academy bilan), fayl biriktirish (Storage bilan), xodimlarning ichki eslatmalari, SignalR, yangi xabar bildirishnomasi va kundalik digest. O'quvchi hisobi o'chirilganda uning yozishmalari Chat'da qoladi; ular event mexanizmi bilan tozalanadi.

## Fayllar (Storage)

Storage faqat fayl baytlarini saqlaydi. Fayl nima ekanini (pasport, transkript, rasm) bilmaydi, buni faylni ishlatadigan modul biladi. Baytlar API orqali o'tmaydi: brauzer faylni imzolangan havola bilan to'g'ridan-to'g'ri S3 omboriga yuklaydi va yuklab oladi. Prod'da bu alohida pullik S3-mos ombor bo'ladi: Cloudflare R2 yoki Hetzner Object Storage, buyurtmachi hali tanlamagan. Lokalda SeaweedFS ishlatiladi (pastda). Bu yo'l spec'dagi talab bo'yicha tanlandi. Muqobili fayllarni API orqali qabul qilib, serverning o'z diskida saqlash edi: alohida ombor kerak bo'lmasdi, lekin hamma fayl serverdan o'tardi, disk, zaxira nusxa va ikkinchi server masalasi bizga qolardi.

**Yuklash oqimi** (login qilgan har qanday foydalanuvchi):

1. `POST /files/upload-url` `{ "fileName", "contentType", "sizeBytes", "purpose" }` — javob: `{ fileId, uploadUrl, method: "PUT", headers, expiresOnUtc }`. Havola 10 daqiqa amal qiladi.
2. Brauzer faylni `uploadUrl` ga `PUT` qiladi va `headers` dagi `Content-Type` ni aynan shunday yuboradi; boshqa tur bilan ombor so'rovni rad etadi.
3. `POST /files/{fileId}/complete` — API ombordagi faylning avval hajmini, keyin birinchi baytlarini tekshiradi. Fayl vaqtinchalik kalitdan (`uploads/<uuid>`) doimiy kalitga (`docs/<uuid>`) ko'chiriladi va `Uploaded` bo'ladi.
4. Fon job'i faylni ClamAV bilan virusga tekshiradi (tasdiqlangandan keyin darhol, zaxira sifatida har daqiqada). Toza fayl `scanStatus: 2` (Clean) va `sha256` oladi. Zararli fayl ombordan o'chiriladi va `scanStatus: 3` (Infected), `Deleted` bo'ladi.
5. `GET /files/{fileId}/download-url` — 5 daqiqalik yuklab olish havolasi, faqat toza fayl uchun. Tekshiruv tugamagan bo'lsa `409 StoredFile.NotScanned` qaytadi, frontend bir necha soniyadan keyin qayta so'raydi. Fayl asl nomi bilan yuklanadi (`Content-Disposition: attachment`).

**Qoidalar va qarorlar.**

- **Turlar va hajm.** Faqat PDF, JPG, PNG va HEIC, har biri 20 MB gacha. Fayl nomining kengaytmasi turga mos bo'lishi kerak (`StoredFile.UnsupportedType`, `StoredFile.TooLarge`, `StoredFile.Empty`).
- **Fayl maqsadi (`purpose`).** Yuklashda fayl nima uchun ekanini aytiladi: `1` — shaxsiy hujjat (standart, `purpose` yuborilmasa ham shu), `2` — ochiq saytdagi rasm (faqat JPG yoki PNG, `StoredFile.UnsupportedMediaType`). Maqsad keyin o'zgarmaydi. `UploadPurpose` migratsiyasidan oldin yuklangan fayllar `1` bo'ladi, ochiq nusxasi borlari esa `2`. Shuning uchun oldin yuklanib, hali biriktirilmagan muqova `409 StoredFile.NotPublicMedia` qaytaradi va qayta yuklanadi. Storage faqat `2` ni ochiq bucket'ga chiqaradi (`StoredFile.NotPublicMedia`), shuning uchun boshqa modulning kodi xato qilsa ham pasport yoki transkript ochiq bo'lib qolmaydi. O'quvchi hujjatini qabul qiladigan modul (Student) esa faqat `1` ni qabul qilishi kerak. Ochiq rasmni kim chiqarishini baribir chaqiruvchi modul hal qiladi: Content faqat admin o'zi yuklagan rasmni ochadi.
- **Mazmun tekshiriladi.** Tasdiqlashda faylning birinchi baytlari (PDF `%PDF-`, PNG va JPEG imzosi, HEIC `ftyp` brendi) va hajmi tekshiriladi. Mos kelmasa, fayl ombordan o'chiriladi va `StoredFile.ContentMismatch` yoki `StoredFile.SizeMismatch` qaytadi. O'quvchi shu `fileId` bilan qayta yuklashi mumkin, lekin uning `uploadUrl` i 10 daqiqadan keyin eskiradi va yangi `upload-url` kerak bo'ladi.
- **Tasdiqlangan faylni almashtirib bo'lmaydi.** Yuklash havolasi tasdiqlangandan keyin ham 10 daqiqa amal qiladi, chunki imzolangan PUT'ni bekor qilib bo'lmaydi. Shuning uchun havola faqat vaqtinchalik kalitga yozadi, tekshirilgan fayl esa havolasi yo'q doimiy kalitga ko'chiriladi. Kimdir tasdiqlangandan keyin shu havola bilan yana yuklasa, u tashlab ketilgan yuklash bo'lib qoladi va tozalanadi.
- **Cheklovlar.** Bir foydalanuvchida bir vaqtda ko'pi bilan 10 ta tasdiqlanmagan yuklash bo'ladi (`StoredFile.TooManyPending`). `upload-url` ga foydalanuvchi boshiga daqiqasiga 20 ta so'rov ruxsat etiladi (`RateLimitPolicies.FileUpload`, oshsa `429`). Imzolangan PUT hajmni yuklash paytida cheklay olmaydi, shuning uchun hajmni tasdiqlash bosqichi tekshiradi, tasdiqlanmagan baytlarni esa tozalash job'i o'chiradi. Prod'da omborga umumiy hajm chegarasini (quota) qo'yish tavsiya etiladi.
- **Kim ko'radi.** Endpoint orqali faylni faqat uni yuklagan odam tasdiqlaydi va yuklab oladi; boshqasiga `404 StoredFile.NotFound`. Mentor yoki admin o'quvchi hujjatini ko'rishi kerak bo'lsa, ruxsatni hujjatni ishlatadigan modul (masalan Student) tekshiradi va Storage'ga o'z adapteri orqali murojaat qiladi.
- **Boshqa modullar uchun API** (endpoint'siz, faqat `ISender` va adapter orqali):
  - `GetStoredFileQuery(fileId)` — fayl ma'lumoti: maqsadi (`purpose`), kim yuklagan, holati, tekshiruv holati, `sha256`, `isReady` (yuklangan va toza). Modul o'quvchi hujjatga faqat o'zi yuklagan va tayyor faylni biriktirayotganini shu bilan tekshiradi.
  - `GetDownloadUrlQuery(fileId)` — egasini tekshirmaydigan 5 daqiqalik havola. Ruxsatni chaqiruvchi modul tekshirgan bo'lishi kerak.
  - `DeleteFileCommand(fileId)` — faylni ombordan (ochiq nusxasi bo'lsa, uni ham) o'chiradi, yozuv `Deleted` va o'chirilgan sana bilan qoladi (offboarding purge). Qayta chaqirish zararsiz.
  - `PublishFileCommand(fileId)` — toza faylni ochiq bucket'ga `public/<uuid>` kaliti bilan nusxalaydi va doimiy URL'ini (`Storage:S3:PublicBaseUrl` + kalit) qaytaradi. Nusxada `Cache-Control: public, max-age=86400` bor: CDN va brauzer rasmni bir kun keshlaydi. Muddat ataylab qisqa: olib tashlangan muqova (masalan noo'rin rasm) brauzer keshidan ko'pi bilan bir kunda yo'qoladi, CDN'da esa darhol tozalash uchun purge qilinadi. Hujjat sifatida yuklangan fayl `409 StoredFile.NotPublicMedia`, tekshiruv tugamagan fayl `409 StoredFile.NotScanned`, tekshirib bo'lmagani `409 StoredFile.ScanFailed` qaytaradi. Allaqachon ochiq faylga o'sha URL qaytadi, qayta nusxalanmaydi. Fayl nima ekanini va uni kim ochishi mumkinligini chaqiruvchi modul tekshiradi (Content faqat admin o'zi yuklagan JPG/PNG'ni ochadi).
  - `UnpublishFileCommand(fileId)` — ochiq nusxani o'chiradi, asl fayl qoladi. Ochiq bo'lmagan fayl uchun zararsiz.
- **Kalitlar tasodifiy.** Ombordagi kalit `docs/<uuid>`: unda fayl nomi yoki o'quvchi ismi yo'q. Asl nom faqat bazada saqlanadi. Bucket yopiq, imzosiz so'rov rad etiladi.
- **Tashlab ketilgan yuklashlar.** Bir soat ichida tasdiqlanmagan yuklashning ikkala kaliti ham ombordan o'chiriladi va u `Deleted` bo'ladi (Hangfire, har soatda `storage-remove-abandoned-uploads`, bir ishga tushishda 500 tagacha).
- **Egasiz ochiq nusxalar.** Ochiq bucket'ga nusxa saqlashdan oldin yoziladi, shuning uchun saqlash muvaffaqiyatsiz bo'lsa, `public/` da hech qaysi fayl ishora qilmaydigan obyekt qolishi mumkin. Hangfire har kuni `storage-remove-orphaned-public-copies` job'ini ishga tushiradi: u `public/` dagi hamma obyektni sahifalab (1 000 tadan) o'qiydi va bazada `public_key` sifatida yo'q, 1 soatdan eski obyektlarni o'chiradi. 1 soat — hali saqlanayotgan nashrni o'chirib yubormaslik uchun. O'chirishdan oldin baza qayta tekshiriladi. Bucket'ning `public/` dan tashqaridagi obyektlariga tegilmaydi.
  - **Ehtiyot chorasi.** Egasiz obyektlar 20 tadan ko'p va ro'yxatdagilarning yarmidan ortiq bo'lsa, job hech narsani o'chirmaydi, xato yozadi va Hangfire'da qayta urinmasdan `Failed` bo'ladi (`StoredFile.OrphanCleanupRefused`). Bu holat odatda baza bilan bucket mos kelmaganini bildiradi: staging prod bucket'iga ulangan yoki baza eski nusxadan tiklangan. Aks holda job hamma jonli muqovalarni o'chirib yuborardi.
  - Bitta obyektni o'chirib bo'lmasa, u log'ga yoziladi va qolganlari o'chirilaveradi. Ombor bo'sh yoki takrorlangan sahifa tokenini qaytarsa, o'qish to'xtaydi.
  - Content'da yangiligi saqlanmay qolgan, lekin Storage'da `public_key` i bor fayl bu job'ga "egali" ko'rinadi. Bunday faylni Content o'zi qaytarib oladi (yuqorida, "Yangiliklar").
- **Virus tekshiruvi.** ClamAV'ning `clamd` xizmati bilan INSTREAM protokoli orqali gaplashiladi (`Storage:ClamAv`: `Host`, `Port`, `TimeoutSeconds`). Fayl S3'dan o'qiladi (20 MB gacha, xotirada), tekshiriladi va o'sha baytlardan `sha256` hisoblanadi. Har bir fayl alohida tekshiriladi: bitta fayl xato bersa (ClamAV ishlamay qoldi, javob bermadi, ombordagi obyekt yo'q), boshqalari tekshirilaveradi, u fayl esa keyingi urinishda navbatning oxiriga o'tadi. Uch marta tekshirib bo'lmagan fayl `scanStatus: 4` (Failed) bo'ladi va yuklab olishda `409 StoredFile.ScanFailed` qaytadi: o'quvchi faylni qaytadan yuklaydi. `clamav/clamd.conf` da `AlertExceedsMax yes`: fayl ClamAV chegaralaridan oshsa, u toza deb o'tkazib yuborilmaydi, zararli deb hisoblanadi. Fayl baytlari clamd'ga shifrlanmagan TCP orqali boradi, clamd esa parol so'ramaydi, shuning uchun prod'da u faqat API bilan bir yopiq tarmoqda (yoki localhost'da) turishi kerak. Yuklab olish shu tekshiruvlar tugaguncha yopiq. Lokalda `docker compose` dagi `clamav` birinchi ishga tushishda virus bazasini yuklab oladi, bu bir necha daqiqa oladi.
- **Hali yo'q:** zararli fayl haqida o'quvchiga xabar (Notification bilan), HEIC'ni JPG'ga o'girish va kichik rasm (thumbnail), xodim yuklab olishini audit'ga yozish (Audit bilan).
- **S3 sozlamalari** `Storage:S3` bo'limida: `ServiceUrl`, `Region`, `AccessKey`, `SecretKey`, `DocumentsBucket`, `PublicBucket`, `PublicBaseUrl`, `ForcePathStyle`. `ForcePathStyle` dan boshqa biri bo'sh bo'lsa, API ishga tushmaydi. Prod kaliti faqat shu ikki bucket'ga yozish va o'qish huquqiga ega bo'lishi, hujjatlar bucket'ining CORS'i esa faqat haqiqiy frontend manzilini ruxsat qilishi kerak. SDK'ning yangi standart checksum'lari o'chirilgan (`WHEN_REQUIRED`), chunki S3-mos omborlar ularni har doim ham qo'llamaydi.
- **Ochiq bucket.** `PublicBucket` dagi obyektlarni imzosiz o'qish mumkin, bucket ro'yxatini esa olib bo'lmaydi. Unga faqat `PublishFileCommand` yozadi. `PublicBucket` `DocumentsBucket` bilan bir xil bo'lsa yoki `PublicBaseUrl` hujjatlar bucket'iga ishora qilsa, API ishga tushmaydi: aks holda sozlamadagi bitta xato pasport va transkriptlarni ochiq qilib qo'yardi. Ochiq nusxa uchun prod'da kerak bo'ladigan sarlavhalar pastdagi "Prod'ga chiqarishdan oldin" ro'yxatida. Prod'da R2 bucket'iga o'z domeni ulanadi, Hetzner'da esa bucket policy faqat `GetObject` ni ochiq qiladi. `PublicBaseUrl` shu manzil bo'ladi. U o'zgarsa, yangiliklardagi `coverUrl` migratsiya bilan yangilanadi, chunki URL to'liq saqlanadi.
- **Lokal ombor SeaweedFS.** MinIO endi tayyor community image chiqarmaydi, shuning uchun lokalda SeaweedFS'ning S3 gateway'i ishlaydi (`docker compose` dagi `seaweedfs`; `seaweedfs-buckets` bir martalik konteyner `fourprep-documents` va `fourprep-public` bucket'larini yaratadi, `seaweedfs/s3.json` dagi `anonymous` identity esa faqat `fourprep-public` dan imzosiz o'qishga ruxsat beradi). Kod faqat S3 API'ni biladi, shuning uchun prod'da R2 ham, Hetzner ham faqat `Storage:S3` sozlamalari bilan ishlaydi. Brauzer `http://localhost:4200` dan yuklashi uchun CORS yoqilgan. Port faqat `127.0.0.1` da ochiq, chunki lokal kalit hammaga ma'lum.

## Prod'ga chiqarishdan oldin

Kod tekshira olmaydigan, lekin prod'da albatta qilinishi kerak bo'lgan sozlamalar.

**Fayl ombori (R2 yoki Hetzner).**

- Ikki bucket: yopiq hujjatlar (`DocumentsBucket`) va ochiq rasmlar (`PublicBucket`). Ochiq bucket'da imzosiz faqat `GetObject` ruxsat etiladi, ro'yxat (`ListBucket`) yopiq. Hujjatlar bucket'i to'liq yopiq.
- Ochiq bucket CDN orqali o'z domenida beriladi, `PublicBaseUrl` — shu domen (`https://...`).
- CDN ochiq rasmlarga quyidagi sarlavhalarni qo'shadi. S3 obyekt metama'lumoti ularni saqlay olmaydi, shuning uchun lokal SeaweedFS ularni bermaydi:
  - `X-Content-Type-Options: nosniff` — brauzer faylni e'lon qilingan turidan (`image/png`, `image/jpeg`) boshqacha talqin qilmaydi, rasm niqobidagi skript ishlamaydi;
  - `Content-Security-Policy: default-src 'none'; img-src 'self'; sandbox` — fayl to'g'ridan-to'g'ri ochilsa ham, unda hech narsa ishga tushmaydi.

  Cloudflare R2'da bu Response Header Transform Rule bilan qilinadi. Hetzner Object Storage o'zi sarlavha qo'sha olmaydi, oldiga CDN (Cloudflare yoki Bunny) qo'yiladi. Tekshirish: `curl -I <coverUrl>` javobida `x-content-type-options: nosniff` bo'lishi kerak.

  Bu sarlavhalar faqat ochiq rasmlar domeniga qo'yiladi, saytning o'ziga emas. Saytning o'z CSP'sidagi `img-src` esa shu CDN domenini o'z ichiga olishi kerak, aks holda muqovalar ko'rinmaydi.
- Ombor provayderining o'z ochiq manzili o'chiriladi (R2'da `r2.dev` subdomeni, Hetzner'da bucket'ning to'g'ridan-to'g'ri ochiq URL'i). Aks holda rasmni CDN'ni chetlab, sarlavhalarsiz ochish mumkin.
- Ochiq bucket'da versiyalash va object lock yoqilmaydi, yoki eski versiyalar lifecycle qoidasi bilan o'chiriladi. Aks holda olib tashlangan muqovalar eski versiya bo'lib saqlanib qoladi.
- Imzosiz yozish (`PutObject`) ikkala bucket'da ham yopiq. Ochiq bucket'ga CORS kerak emas.
- Noo'rin rasm olib tashlanganda CDN keshi purge qilinadi, aks holda u bir kungacha (`max-age=86400`) ko'rinib turadi.
- API kaliti faqat shu ikki bucket'ga ruxsatga ega: ikkalasida o'qish, yozish va o'chirish, ochiq bucket'da esa ro'yxat (`ListBucket`) ham, chunki tozalash job'i shunga tayanadi. Hujjatlar bucket'ining CORS'i faqat haqiqiy frontend manzilini ruxsat qiladi.
- Hujjatlar bucket'ida (voyaga yetmaganlarning pasportlari shu yerda) shifrlash (server-side encryption) yoqiladi, hech qanday ochiq ACL yoki policy bo'lmaydi, kirish log'i yoqiladi, tugallanmagan multipart yuklashlar va 1 kundan eski `uploads/` obyektlari lifecycle qoidasi bilan tozalanadi (yangi yuklashlar `uploads/` da 10 daqiqagacha turadi, shuning uchun qoida ularga tegmasligi kerak).
- Ombor hajmiga quota yoki ogohlantirish qo'yiladi.
- Hangfire'dagi `Failed` job'lar (ayniqsa `storage-remove-orphaned-public-copies` va `storage-scan-uploaded-files`) kuzatiladi va ular haqida xabar keladi.
- Storage bazasi eski nusxadan tiklansa, avval `storage-remove-orphaned-public-copies` job'i to'xtatiladi. Ehtiyot chorasi ko'p holatni ushlaydi, lekin hammasini emas.

**ClamAV.** `clamd` faqat API bilan bir yopiq tarmoqda (yoki localhost'da) turadi, chunki u parol so'ramaydi va baytlar shifrlanmagan TCP orqali boradi.

## Butun dunyodan foydalanuvchilar

O'quvchilar faqat O'zbekistondan emas, shuning uchun hech narsa O'zbekistonga qattiq bog'lanmagan.

- **Ro'yxatdan o'tish.** `POST /auth/register` va `/auth/register-parent` qo'shimcha maydonlarni oladi: `country` (majburiy, ISO 3166-1 alpha-2, masalan `US`), `timeZone` (ixtiyoriy, IANA nomi, masalan `America/New_York`; brauzerda `Intl.DateTimeFormat().resolvedOptions().timeZone`) va `locale` (ixtiyoriy, `en`, `uz` yoki `ru`). Telefon raqami ixtiyoriy, lekin bo'lsa xalqaro formatda (E.164) bo'lishi kerak: `+1 (415) 555-0100` qabul qilinadi va `+14155550100` bo'lib saqlanadi.
- **Token.** Bu qiymatlar Keycloak atributlarida saqlanadi va access token'ga `country`, `zoneinfo`, `locale` claim'lari bo'lib tushadi. `ITimeZoneContext` foydalanuvchining o'z vaqt zonasini `zoneinfo` dan oladi; u bo'lmasa UTC ishlatiladi.
- **O'z hisobi.** `GET /auth/me` hisobni qaytaradi, `PUT /auth/me` telefon, davlat, vaqt zonasi va tilni o'zgartiradi. Yangi qiymatlar token'ga keyingi `POST /auth/refresh` da tushadi.
- **O'quvchi profili.** `Region` enum'i (viloyatlar) olib tashlandi. Profilda endi `country` (ISO kod, ro'yxatdan o'tishdagi davlat bilan boshlanadi) va erkin matnli `region` (shtat, viloyat, provinsiya) bor. Ro'yxatlar `country` bo'yicha filtrlanadi. Sinf 1 dan 12 gacha. Migratsiya eski viloyatlarni `UZ` va viloyat nomiga ko'chiradi.
- **Xato xabarlari.** Javobdagi `messages` da har doim `en`, `uz`, `ru` matnlari bor; frontend foydalanuvchi tilidagisini ko'rsatadi.

## O'z hisobini boshqarish

- **Parolni o'zgartirish.** `PUT /auth/me/password` `{ "currentPassword", "newPassword" }`. Joriy parol noto'g'ri bo'lsa `User.WrongPassword`, yangisi eskisi bilan bir xil bo'lsa `User.PasswordUnchanged` qaytadi. Muvaffaqiyatli bo'lsa, barcha qurilmalardagi sessiyalar yopiladi va javobda shu qurilma uchun yangi token'lar keladi; frontend ularni saqlab qo'yishi kerak. Faqat Google bilan kiradigan hisobda parol yo'q: `User.PasswordNotSet` qaytadi, parolni "Parolni unutdim" orqali yaratadi.
- **Hisobni o'chirish.** `DELETE /auth/me` `{ "password" }` (paroli yo'q hisob body yubormasa ham bo'ladi), faqat `student` va `parent` uchun (xodimni `super_admin` o'chiradi). Keycloak hisobi, o'quvchi profili va imtihon maqsadlari, ota-ona bog'lanishlari va sessiya yozuvlari darhol o'chadi; qaytarib bo'lmaydi. Paroli bor hisob parol bilan tasdiqlaydi. Paroli yo'q (faqat Google) hisob uchun so'nggi 10 daqiqada ochilgan sessiya kerak: aks holda `User.ReauthenticationRequired` qaytadi va frontend foydalanuvchini Google bilan qayta kirgizib, so'rovni takrorlaydi. Google token'ini o'chirish so'rovida tasdiq sifatida ishlatmadik: Keycloak'ning Google oqimi yangi hisob ochishi yoki emaili tasdiqlanmagan hisobni egallab olishi mumkin, bu esa tasdiq uchun xavfli yon ta'sir.
- **Email tasdiqlangani.** Tasdiqlanmagan email bilan ham login va ro'yxatdan o'tish ishlaydi. Tasdiqlangan email talab qiladigan endpoint (masalan, keyingi to'lov yoki hujjat modullarida) `Policies.VerifiedEmail` ni qo'yadi: u token'dagi `email_verified` claim'ini tekshiradi. Foydalanuvchi emailni tasdiqlagach, yangi claim token'ga keyingi refresh'da tushadi.

## Xodimlar uchun hisob boshqaruvi (`super_admin`)

- `DELETE /admin/users/{id}` hisobni va uning Student modulidagi ma'lumotlarini o'chiradi; mentor o'chirilsa, uning o'quvchilaridan mentor olib tashlanadi. Admin o'zini o'chira olmaydi (`User.CannotDeleteSelf`).
- `PUT /admin/users/{id}/password` `{ "password" }` yangi parol qo'yadi va foydalanuvchining barcha sessiyalarini yopadi.
- `PUT /admin/users/{id}/contact` `{ "email", "phoneNumber" }` kontaktni o'zgartiradi; email o'zgarsa, u tasdiqlanmagan bo'lib qoladi.

## Email: tasdiqlash va parolni tiklash

SMS butun dunyoda ishonchli emas, shuning uchun ikkalasi ham email orqali, Keycloak'ning o'z xatlari bilan ishlaydi. Xat foydalanuvchining `locale` tilida keladi.

- **Tasdiqlash.** Ro'yxatdan o'tgandan keyin tasdiqlash xati darhol yuboriladi (yuborilmasa ham ro'yxatdan o'tish bekor bo'lmaydi). Qayta yuborish: `POST /auth/me/verification-email`. Havola 24 soat amal qiladi. Hozircha login uchun email tasdiqlangan bo'lishi shart emas.
- **Parolni tiklash.** `POST /auth/forgot-password` `{ "email": "..." }` qabul qiladi va har doim `200` qaytaradi, shunda bu endpoint orqali kim ro'yxatdan o'tganini bilib bo'lmaydi. Havola 1 soat amal qiladi.
- **Cheklov.** Ikkala endpoint bitta IP manzildan 15 daqiqada 5 tadan ko'p chaqirilsa, `429` qaytaradi. API proxy yoki load balancer ortida tursa, uning manzilini `ForwardedHeaders:KnownProxies` (masalan `["10.0.0.5"]`) yoki `ForwardedHeaders:KnownNetworks` (masalan `["10.0.0.0/8"]`) ga yozing. Shunda cheklov proxy'ning emas, mijozning haqiqiy IP manzilini (`X-Forwarded-For`) ko'radi. Ro'yxatda yo'q manzildan kelgan `X-Forwarded-For` hisobga olinmaydi.
- **Sozlama.** `Identity:Keycloak:EmailRedirectUrl` xatdagi "ilovaga qaytish" havolasi; u `fourprep-api` client'ining redirect URI'lari ichida bo'lishi kerak. Lokalda xatlar Mailpit'ga tushadi. Prod'da realm'ning SMTP sozlamasini haqiqiy pochta serveriga almashtiring.

## Google orqali kirish

Frontend Google'ning o'z kutubxonasidan (web uchun Google Identity Services) `id_token` oladi va `POST /auth/google` ga yuboradi. Frontend Keycloak bilan gaplashmaydi. Javob `/auth/login` niki bilan bir xil, shuning uchun refresh, logout va logout-all o'zgarishsiz ishlaydi.

```json
{ "idToken": "eyJ...", "role": "student", "country": "US", "timeZone": "America/New_York", "locale": "en" }
```

- **Kim kira oladi.** Faqat foydalanuvchi panelidagilar: `student` va `parent`. Xodimlar (`mentor`, `teacher`, `mentor_admin`, `content_admin`, `super_admin`) Google orqali na ro'yxatdan o'ta oladi, na kira oladi; ular xodimlar paneliga faqat login va parol bilan kiradi. Xodim o'z emaili bilan Google orqali kirmoqchi bo'lsa, `403 Authentication.GoogleSignInNotAllowed` qaytadi, hisob Google'ga bog'lanmaydi va paroli o'zgarmaydi. Bitta hisobda foydalanuvchi va xodim roli birga bo'lsa ham rad etiladi.
- **Birinchi kirish.** Keycloak'da bu Google hisobi uchun foydalanuvchi ochiladi, lekin unda hali rol yo'q. So'rovda `role` (`student` yoki `parent`) va `country` bo'lmasa, API `400 Authentication.SignUpDetailsRequired` qaytaradi va ochilgan sessiyani yopadi. Frontend shu xatoni olganda rol va davlatni so'raydi, keyin o'sha `idToken` bilan qayta yuboradi (Google token'i 1 soat amal qiladi). `timeZone` va `locale` ixtiyoriy. Rol berilgach, o'quvchiga profil ochiladi, xuddi `/auth/register` dagidek.
- **Frontend uchun tavsiya.** `timeZone` (`Intl.DateTimeFormat().resolvedOptions().timeZone`) va `locale` (interfeys tili) ni so'ramasdan har doim yuboring. Ro'yxatdan o'tish sahifasida "O'quvchi / Ota-ona" tanlovi va davlat Google tugmasidan oldin bo'lsa, birinchi kirish bitta so'rovda tugaydi. Kirish sahifasida esa faqat `idToken` yuboring va `SignUpDetailsRequired` kelsa, rol va davlatni so'rang.
- **Keyingi kirishlar.** Faqat `idToken` kerak. `role`, `country` va boshqalar e'tiborga olinmaydi.
- **Username.** `google-<Google sub>`, masalan `google-108234567890`. Email va ism Google token'idan olinadi; Google tasdiqlagan email tasdiqlangan deb belgilanadi, shuning uchun tasdiqlash xati yuborilmaydi.
- **Xatolar.** Soxta, muddati o'tgan yoki boshqa client uchun chiqarilgan token, shuningdek bloklangan hisob — hammasi `Authentication.InvalidGoogleToken`. Parol bilan kirishda ham bloklangan hisob noto'g'ri parol bilan bir xil ko'rinadi.

### Hisoblar qanday bog'lanadi

Har qadamda topilgan hisobning rollari tekshiriladi: realm'ning standart rollaridan tashqari `student` va `parent` dan boshqa roli bo'lsa, hisob rad etiladi va hech narsa o'zgartirilmaydi.

1. Google hisobi avval bog'langan bo'lsa, o'sha foydalanuvchi.
2. Aks holda Google tasdiqlagan email bo'yicha mavjud hisob qidiriladi va Google'ga bog'lanadi. Shu hisobning emaili tasdiqlanmagan bo'lsa, u **egallab olinadi**: barcha parollari o'chiriladi, barcha sessiyalari (offline ham) yopiladi va email tasdiqlangan bo'ladi. Sabab: `/auth/register` emailni tasdiqlamaydi, shuning uchun istalgan odam birovning manzili bilan parolli hisob ochib qo'yishi mumkin. Haqiqiy egasi Google bilan kirganda bu hisobga u kira olmay qoladi. Emaili tasdiqlangan hisob parolini saqlaydi.
3. Aks holda yangi foydalanuvchi ochiladi.

### Nega Keycloak extension

Google token'ini oddiy Keycloak sessiyasiga (refresh token bilan) aylantirishning Keycloak'da tayyor yo'li yo'q: JWT Authorization Grant (RFC 7523) refresh token bermaydi, eski external token exchange esa preview holatida va undan voz kechilmoqda. Shuning uchun `keycloak/fourprep-keycloak-extensions` dagi `fourprep-external-id-token` authenticator'i ishlatiladi. U tofan'dan olingan: Apple uchun kerak bo'lgan qismi olib tashlangan, xodimlarni rad etish qo'shilgan.

- API parol bilan kirishdagi password grant'ni yuboradi, faqat parol o'rniga `external_provider=google` va `external_id_token` bilan. Keycloak bu grant uchun `username` ni majburiy qiladi, shuning uchun unga `google` yoziladi; authenticator uni e'tiborsiz qoldiradi.
- Realm'ning direct grant oqimi `fourprep direct grant`: birinchi muqobil — authenticator, ikkinchisi — oddiy username/parol (OTP bilan). Google parametrlari bo'lmasa, so'rov parol yo'liga o'tadi.
- Qaysi rollar Google ishlata olishi authenticator'ning `allowedRoles` sozlamasida (realm faylida `fourprep external sign-in roles`, hozir `student##parent`). Yangi foydalanuvchi roli qo'shilsa, shu yerga ham yozing. Sozlama bo'lmasa, authenticator Google kirishning hammasini rad etadi. tofan'dagi extension'da bu tekshiruv yo'q, u FourPrep uchun qo'shilgan. API ham Keycloak'dan keyin rollarni qayta tekshiradi.
- Token imzosi Keycloak'ning kalit keshi orqali Google'ning JWKS'i bo'yicha tekshiriladi, keyin `iss`, `aud`, `sub` va `exp`. `azp` tekshirilmaydi, chunki keyinchalik mobil ilova token'ida `azp` Android/iOS client bo'ladi, `aud` esa Web client. Provider'da `validateSignature` o'chirilgan bo'lsa, authenticator hamma token'ni rad etadi.
- Authenticator Keycloak'ning ichki SPI'sida yozilgan. Keycloak har ko'tarilganda extension'ni yangi versiyaga qarshi yig'ib, Google bilan kirishni qayta tekshiring. Ishga tushishda `KC-SERVICES0047 ... internal SPI` ogohlantirishi chiqadi — bu kutilgan holat, extension yuklangan degani.

### Sozlash

Google Cloud Console → **APIs & Services → Credentials → Create credentials → OAuth client ID** → turi **Web application**, **Authorized JavaScript origins** ga frontend manzili (lokalda `http://localhost:4200`). Client secret kerak emas: Keycloak faqat token'ning `aud` qiymatini shu client ID bilan solishtiradi.

Client ID'ni repo ildizidagi `.env` fayliga yozing (u git'ga tushmaydi):

```bash
GOOGLE_CLIENT_ID=1234567890-abc.apps.googleusercontent.com
```

Realm import paytida `${GOOGLE_CLIENT_ID}` shu qiymat bilan almashtiriladi. Realm allaqachon import qilingan bo'lsa, `fourprep_keycloak` bazasini qayta yarating yoki Keycloak konsolida **Identity providers → google → Client ID** ni o'zgartiring. Frontend ham Google kutubxonasini shu client ID bilan ishga tushiradi.

## Dasturchilar uchun eslatmalar

Kodda izoh yozilmaydi (`CLAUDE.md`, 6.9.1). Kodning o'zidan ko'rinmaydigan sabablar shu yerda.

- **Vaqt zonasi faqat IANA nomi bilan.** .NET Windows nomlarini ham (`Pacific Standard Time`) taniydi, lekin `TimeZones` ularni rad etadi: web, mobil ilova va token zonani bir xil yozishi kerak. Foydalanuvchi zonasi noma'lum bo'lsa va so'rovdan tashqarida ishlaydigan job'larda UTC ishlatiladi.
- **Telefon E.164 da saqlanadi.** Odamlar yozadigan ajratuvchilar olib tashlanadi, shunda istalgan davlat raqamlari bir xil solishtiriladi va qidiriladi.
- **Keycloak atributlari.** Yangilashda Keycloak hisobdagi barcha atributlarni almashtiradi. Shuning uchun yangilash hisobdagi mavjud qiymatlardan (`UserAttributes.Of`) boshlanadi, aks holda tegilmagan atributlar o'chib ketadi.
- **Username kichik harfda.** Keycloak username'ni kichik harfda saqlaydi. O'quvchi profili ham shunday yoziladi, shunda u token'dagi `preferred_username` bilan mos keladi.
- **Bog'lash kodi.** Kodda `0`/`O` va `1`/`I`/`L` yo'q: ota-ona uni o'quvchi ekranidan qo'lda teradi.
