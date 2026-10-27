# FourPrep

## Lokal ishga tushirish

Kerak bo'ladi: .NET 10 SDK, Docker va `localhost:5432` da ishlab turgan PostgreSQL (`postgres` / `postgres`).

1. Keycloak uchun bazani bir marta yarating:

   ```bash
   psql -h localhost -U postgres -c "CREATE DATABASE fourprep_keycloak"
   ```

2. Keycloak va Mailpit'ni ishga tushiring:

   ```bash
   docker compose up -d
   ```

   Birinchi ishga tushishda `keycloak/realms/fourprep-realm.json` dagi `fourprep` realm'i import qilinadi. Unda 7 ta rol, `fourprep-api` client'i (audience va `country` mapper'lari, service account huquqlari bilan), token muddatlari, tillar (en, uz, ru), SMTP sozlamasi va User Profile bor. Realm allaqachon mavjud bo'lsa, import o'tkazib yuboriladi. Fayldagi o'zgarishni qo'llash uchun `fourprep_keycloak` bazasini o'chirib, qayta yarating.

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

## Keycloak bo'yicha eslatmalar

- **User Profile.** Keycloak 26 standart holatda `firstName` va `lastName` ni majburiy qiladi. Ism-familiyasiz yaratilgan foydalanuvchi login paytida `Account is not fully set up` xatosini oladi. Realm faylida bu ikki maydon ixtiyoriy qilingan.
- **Atributlar.** `phone_number`, `country` va `zoneinfo` User Profile'da e'lon qilingan. E'lon qilinmagan atributni Keycloak hech qanday xatosiz tashlab yuboradi. `locale` alohida e'lon qilinmaydi: realm'da tillar yoqilgani uchun Keycloak uni o'zi boshqaradi.
- **Rollar.** `POST /auth/register` orqali ro'yxatdan o'tgan foydalanuvchi `student` rolini oladi, `POST /auth/register-parent` orqali o'tgan esa `parent` rolini. Xodimlarni `super_admin` `POST /admin/users` orqali kerakli rol bilan yaratadi.

## O'quvchi, ota-ona va mentor

- **Profil.** `student` rolini olgan har bir hisob (o'zi ro'yxatdan o'tgan yoki admin yaratgan) uchun Student modulida bo'sh profil va 8 belgili bog'lash kodi ochiladi. Profil yaratilmasa, Keycloak hisobi ham o'chiriladi.
- **Ota-onani bog'lash.** O'quvchi kodni `GET /students/me` dan ko'radi va ota-onasiga beradi. Ota-ona kodni `POST /parents/me/students` ga yuboradi. Bitta o'quvchiga ko'pi bilan 4 ta ota-ona bog'lanadi. O'quvchi `POST /students/me/link-code` bilan yangi kod olsa, eski kod ishlamay qoladi.
- **Mentor.** `mentor_admin` `PUT /students/{id}/mentor` bilan faqat `mentor` rolidagi foydalanuvchini biriktira oladi. Mentor o'z o'quvchilarini `GET /mentors/me/students` orqali ko'radi.
- **Modullararo chaqiruvlar.** Ular tofan'dagidek adapter loyihalar orqali o'tadi: `FourPrep.Modules.Auth.Student` (register → profil) va `FourPrep.Modules.Student.Auth` (mentor rolini tekshirish).

## Butun dunyodan foydalanuvchilar

O'quvchilar faqat O'zbekistondan emas, shuning uchun hech narsa O'zbekistonga qattiq bog'lanmagan.

- **Ro'yxatdan o'tish.** `POST /auth/register` va `/auth/register-parent` qo'shimcha maydonlarni oladi: `country` (majburiy, ISO 3166-1 alpha-2, masalan `US`), `timeZone` (ixtiyoriy, IANA nomi, masalan `America/New_York`; brauzerda `Intl.DateTimeFormat().resolvedOptions().timeZone`) va `locale` (ixtiyoriy, `en`, `uz` yoki `ru`). Telefon raqami ixtiyoriy, lekin bo'lsa xalqaro formatda (E.164) bo'lishi kerak: `+1 (415) 555-0100` qabul qilinadi va `+14155550100` bo'lib saqlanadi.
- **Token.** Bu qiymatlar Keycloak atributlarida saqlanadi va access token'ga `country`, `zoneinfo`, `locale` claim'lari bo'lib tushadi. `ITimeZoneContext` foydalanuvchining o'z vaqt zonasini `zoneinfo` dan oladi; u bo'lmasa UTC ishlatiladi.
- **O'z hisobi.** `GET /auth/me` hisobni qaytaradi, `PUT /auth/me` telefon, davlat, vaqt zonasi va tilni o'zgartiradi. Yangi qiymatlar token'ga keyingi `POST /auth/refresh` da tushadi.
- **O'quvchi profili.** `Region` enum'i (viloyatlar) olib tashlandi. Profilda endi `country` (ISO kod, ro'yxatdan o'tishdagi davlat bilan boshlanadi) va erkin matnli `region` (shtat, viloyat, provinsiya) bor. Ro'yxatlar `country` bo'yicha filtrlanadi. Sinf 1 dan 12 gacha. Migratsiya eski viloyatlarni `UZ` va viloyat nomiga ko'chiradi.
- **Xato xabarlari.** Javobdagi `messages` da har doim `en`, `uz`, `ru` matnlari bor; frontend foydalanuvchi tilidagisini ko'rsatadi.

## Email: tasdiqlash va parolni tiklash

SMS butun dunyoda ishonchli emas, shuning uchun ikkalasi ham email orqali, Keycloak'ning o'z xatlari bilan ishlaydi. Xat foydalanuvchining `locale` tilida keladi.

- **Tasdiqlash.** Ro'yxatdan o'tgandan keyin tasdiqlash xati darhol yuboriladi (yuborilmasa ham ro'yxatdan o'tish bekor bo'lmaydi). Qayta yuborish: `POST /auth/me/verification-email`. Havola 24 soat amal qiladi. Hozircha login uchun email tasdiqlangan bo'lishi shart emas.
- **Parolni tiklash.** `POST /auth/forgot-password` `{ "email": "..." }` qabul qiladi va har doim `200` qaytaradi, shunda bu endpoint orqali kim ro'yxatdan o'tganini bilib bo'lmaydi. Havola 1 soat amal qiladi.
- **Cheklov.** Ikkala endpoint bitta IP manzildan 15 daqiqada 5 tadan ko'p chaqirilsa, `429` qaytaradi. Prod'da API proxy ortida tursa, haqiqiy IP uchun forwarded header'larni sozlash kerak.
- **Sozlama.** `Identity:Keycloak:EmailRedirectUrl` xatdagi "ilovaga qaytish" havolasi; u `fourprep-api` client'ining redirect URI'lari ichida bo'lishi kerak. Lokalda xatlar Mailpit'ga tushadi. Prod'da realm'ning SMTP sozlamasini haqiqiy pochta serveriga almashtiring.

## Dasturchilar uchun eslatmalar

Kodda izoh yozilmaydi (`CLAUDE.md`, 6.9.1). Kodning o'zidan ko'rinmaydigan sabablar shu yerda.

- **Vaqt zonasi faqat IANA nomi bilan.** .NET Windows nomlarini ham (`Pacific Standard Time`) taniydi, lekin `TimeZones` ularni rad etadi: web, mobil ilova va token zonani bir xil yozishi kerak. Foydalanuvchi zonasi noma'lum bo'lsa va so'rovdan tashqarida ishlaydigan job'larda UTC ishlatiladi.
- **Telefon E.164 da saqlanadi.** Odamlar yozadigan ajratuvchilar olib tashlanadi, shunda istalgan davlat raqamlari bir xil solishtiriladi va qidiriladi.
- **Keycloak atributlari.** Yangilashda Keycloak hisobdagi barcha atributlarni almashtiradi. Shuning uchun yangilash hisobdagi mavjud qiymatlardan (`UserAttributes.Of`) boshlanadi, aks holda tegilmagan atributlar o'chib ketadi.
- **Username kichik harfda.** Keycloak username'ni kichik harfda saqlaydi. O'quvchi profili ham shunday yoziladi, shunda u token'dagi `preferred_username` bilan mos keladi.
- **Bog'lash kodi.** Kodda `0`/`O` va `1`/`I`/`L` yo'q: ota-ona uni o'quvchi ekranidan qo'lda teradi.
