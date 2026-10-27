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
