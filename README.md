# FourPrep

## Lokal ishga tushirish

Kerak bo'ladi: .NET 10 SDK, Docker va `localhost:5432` da ishlab turgan PostgreSQL (`postgres` / `postgres`).

1. Keycloak uchun bazani bir marta yarating:

   ```bash
   psql -h localhost -U postgres -c "CREATE DATABASE fourprep_keycloak"
   ```

2. Keycloak va Mailpit'ni ishga tushiring:

   ```bash
   docker compose up -d --build
   ```

   Keycloak rasmiy image emas: `keycloak/Dockerfile` unga Google orqali kirish uchun extension qo'shadi (pastda "Google orqali kirish" bo'limi). Birinchi yig'ish Maven bilan bir necha daqiqa oladi va extension'ning unit testlarini ham ishga tushiradi.

   Birinchi ishga tushishda `keycloak/realms/fourprep-realm.json` dagi `fourprep` realm'i import qilinadi. Unda 7 ta rol, `fourprep-api` client'i (audience va `country` mapper'lari, service account huquqlari bilan), token muddatlari, tillar (en, uz, ru), SMTP sozlamasi, User Profile, `google` identity provider'i va `fourprep direct grant` oqimi bor. Realm allaqachon mavjud bo'lsa, import o'tkazib yuboriladi. Fayldagi o'zgarishni qo'llash uchun `fourprep_keycloak` bazasini o'chirib, qayta yarating.

3. API'ni ishga tushiring:

   ```bash
   dotnet run --project src/API/FourPrep.Api --launch-profile http
   ```

   Swagger: http://localhost:5179/swagger. Migratsiyalar ishga tushishda avtomatik qo'llanadi.

## Faqat lokal muhit uchun ma'lumotlar

| Nima | Qiymat |
|---|---|
| Keycloak admin konsoli | http://localhost:8080, `admin` / `admin` |
| `super_admin` foydalanuvchi | `superadmin` / `superadmin` |
| `fourprep-api` client secret | `fourprep-dev-secret` |
| Mailpit (yuborilgan xatlar) | http://localhost:8025 |

Bu qiymatlarni prod'da ishlatmang.

## Modullar

Har bir modul o'z PostgreSQL sxemasiga ega va boshqa modulga faqat adapter orqali murojaat qiladi (`CLAUDE.md`, 3.3).

| Modul | Nima uniki | Holati |
|---|---|---|
| Auth | Keycloak hisoblari, login, sessiyalar, o'z hisobi, xodimlar hisoblari | tayyor |
| Student | o'quvchi profili, ota-ona bog'lanishi, mentor; keyinroq o'quvchining bosqichi (stage) va yo'li, test ballari, shaxsiy hujjatlari va offboarding | profil, ota-ona va mentor tayyor |
| Catalogue | universitetlar, dasturlar, qabul talablari, narxlar, muddatlar, grantlar, yo'nalish davlatlari | tayyor: qo'lda kiritish, CSV import, ochiq qidiruv |
| Content | yangiliklar, natijalar (results), hamkorlar, About, xodimlarning ochiq profillari | skeleton |
| Academy | kurslar, guruhlar, yozilish, davomat, mock ballar, placement test | skeleton |
| Admission | academy arizasi, universitetlarga arizalar, portal akkauntlari, essay, tavsiyanomalar, universitet offerlari, viza | skeleton |
| Agreement | mahsulotlar, offer va chegirmalar, shartnomalar, imzolar | skeleton |
| Payment | invoice, to'lovlar, to'lov tizimining webhook'i | skeleton |
| Meeting | konsultatsiyalar, xodimlarning bo'sh vaqtlari, video xonalar, mock interview | skeleton |
| AI | AI Counselor, plan wizard va My Plan, haftalik check-in | skeleton |
| Chat | suhbatlar va xabarlar | skeleton |
| Notification | ilova ichidagi, email va Telegram bildirishnomalari | skeleton |
| Storage | faqat fayl baytlari: obyektlar, yuklash va yuklab olish uchun imzolangan havolalar, o'chirish | skeleton |
| Audit | barcha modullardagi muhim amallarning faqat qo'shiladigan (append-only) jurnali; hech qachon o'chirilmaydi | skeleton |

Nega shunday bo'lindi:

- **Catalogue Content'dan alohida.** AI universitetlar haqida faqat katalogdagi faktlarga tayanadi, har bir faktning manbasi (`source_url`) va tekshirilgan sanasi bor, eskirsa ogohlantiriladi. Yangilik va natijalarda bunday qoidalar yo'q. Ikkalasini `content_admin` boshqaradi, lekin ma'lumot va qoidalar har xil, AI esa faqat katalogga adapter orqali murojaat qiladi. Bitta Content moduli ham ko'rib chiqildi: unda AI yangiliklar bilan bir modulga bog'lanib qolardi.
- **Hujjatlar Student'da.** Pasport, transkript, sertifikat o'quvchining yozuvi: turi, holati (yuklangan, tasdiqlangan, rad etilgan), kim tekshirgani va qachon o'chirilishi Student'da saqlanadi. Storage faqat baytlarni saqlaydi va fayl nima ekanini bilmaydi, Student hujjati Storage'dagi fayl id'siga ishora qiladi. Offboarding'da hujjatlarni o'chirishni ham Student boshqaradi: yozuv qoladi, fayl o'chadi. Hujjatlarni Storage'ga yoki alohida modulga berish ko'rib chiqildi: Storage texnik ombor bo'lib qolishi kerak, alohida modul esa offboarding'ni ikki modulga bo'lib yuborardi.
- **Meeting alohida.** Konsultatsiyani Admission, mock interview'ni Admission va Academy, darslarni Academy ishlatadi. Uchrashuv, xodimning bo'sh vaqti va video xona bitta joyda turadi, qolgan modullar faqat uchrashuv id'sini saqlaydi.
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
- **jsonb o'qish.** `CatalogueModule` Dapper'ga `Dictionary<string, string>` va `Dictionary<Exam, decimal>` uchun JSON type handler qo'shadi. Dapper handler'lari butun jarayon uchun umumiy: boshqa modul shu turlarni Dapper bilan o'qisa, ular ham JSON deb o'qiladi.

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
