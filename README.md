# FourPrep

## Lokal ishga tushirish

Kerak bo'ladi: .NET 10 SDK, Docker va `localhost:5432` da ishlab turgan PostgreSQL (`postgres` / `postgres`).

1. Keycloak uchun bazani bir marta yarating:

   ```bash
   psql -h localhost -U postgres -c "CREATE DATABASE fourprep_keycloak"
   ```

2. Keycloak'ni ishga tushiring:

   ```bash
   docker compose up -d
   ```

   Birinchi ishga tushishda `keycloak/realms/fourprep-realm.json` dagi `fourprep` realm'i import qilinadi. Unda 7 ta rol, `fourprep-api` client'i (audience mapper va service account huquqlari bilan), token muddatlari va User Profile bor. Realm allaqachon mavjud bo'lsa, import o'tkazib yuboriladi. Fayldagi o'zgarishni qo'llash uchun `fourprep_keycloak` bazasini o'chirib, qayta yarating.

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

Bu qiymatlarni prod'da ishlatmang.

## Keycloak bo'yicha eslatmalar

- **User Profile.** Keycloak 26 standart holatda `firstName` va `lastName` ni majburiy qiladi. Ism-familiyasiz yaratilgan foydalanuvchi login paytida `Account is not fully set up` xatosini oladi. Realm faylida bu ikki maydon ixtiyoriy qilingan.
- **`phone_number` atributi.** User Profile'da e'lon qilingan. E'lon qilinmasa, Keycloak uni hech qanday xatosiz tashlab yuboradi.
- **Rollar.** `POST /auth/register` orqali ro'yxatdan o'tgan foydalanuvchi `student` rolini oladi, `POST /auth/register-parent` orqali o'tgan esa `parent` rolini. Xodimlarni `super_admin` `POST /admin/users` orqali kerakli rol bilan yaratadi.

## O'quvchi, ota-ona va mentor

- **Profil.** `student` rolini olgan har bir hisob (o'zi ro'yxatdan o'tgan yoki admin yaratgan) uchun Student modulida bo'sh profil va 8 belgili bog'lash kodi ochiladi. Profil yaratilmasa, Keycloak hisobi ham o'chiriladi.
- **Ota-onani bog'lash.** O'quvchi kodni `GET /students/me` dan ko'radi va ota-onasiga beradi. Ota-ona kodni `POST /parents/me/students` ga yuboradi. Bitta o'quvchiga ko'pi bilan 4 ta ota-ona bog'lanadi. O'quvchi `POST /students/me/link-code` bilan yangi kod olsa, eski kod ishlamay qoladi.
- **Mentor.** `mentor_admin` `PUT /students/{id}/mentor` bilan faqat `mentor` rolidagi foydalanuvchini biriktira oladi. Mentor o'z o'quvchilarini `GET /mentors/me/students` orqali ko'radi.
- **Modullararo chaqiruvlar.** Ular tofan'dagidek adapter loyihalar orqali o'tadi: `FourPrep.Modules.Auth.Student` (register → profil) va `FourPrep.Modules.Student.Auth` (mentor rolini tekshirish).
