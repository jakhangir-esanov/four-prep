# CLAUDE.md

## 1. Purpose of this file

This file tells Claude how to work in the FourPrep repository.

FourPrep was set up in the same structure as the sibling repository `tofan` (`D:\Projects\tofan`), and
code is often ported from there. Port the *shape*, not the content: tofan is a fitness platform for
one country, FourPrep is an exam-preparation platform for students worldwide. Anything tofan-specific
(fitness concepts, Uzbekistan-only data, features FourPrep does not use) must not come along.

The goal is code that matches the existing architecture and conventions of this repository, not code
that "works somehow". If a request is ambiguous, choose the option that keeps the architecture
consistent. Ask only when a business requirement is genuinely unclear.

The user writes in Uzbek; answer in Uzbek. Code, identifiers, commit messages and this file are in
English. `README.md` is in Uzbek.

---

## 2. Project identity

FourPrep prepares students for international exams (`Exam` enum in Common: SAT, IELTS, TOEFL,
Duolingo). Students come from any country, not only Uzbekistan.

People in the system, as Keycloak realm roles (`Roles` in Common):
- `student` — signs up through `POST /auth/register`
- `parent` — signs up through `POST /auth/register-parent`, links to a child with the student's link code
- `mentor`, `teacher` — staff
- `mentor_admin` — assigns mentors to students
- `content_admin` — manages learning content
- `super_admin` — creates staff accounts through `POST /admin/users`

Backend stack:
- .NET 10, C# 14, ASP.NET Core minimal APIs
- EF Core for writes only, Dapper for reads only
- PostgreSQL / Npgsql, snake_case naming
- MediatR, FluentValidation
- Serilog
- Keycloak 26 (OAuth2 / OIDC, JWT bearer), run locally through `docker-compose.yml`
- Hangfire (PostgreSQL storage, registered by the Auth module)
- Mailpit as the local SMTP server for Keycloak's emails

The frontend is not in this repository. The API allows `http://localhost:4200` in Development.

Architecture style: Clean Architecture inside a modular monolith, built so a module can later be
extracted into its own service. Identity stays centralized in Keycloak for the same reason.

How to run it locally, the dev credentials and the Keycloak notes are in `README.md`.

---

## 3. Core architectural philosophy

### 3.1 Layers

Every module has four projects:

- Domain — depends only on `FourPrep.Common.Domain`
- Application — depends on its Domain and Common.Application
- Infrastructure — implements Application and Domain contracts, registers the module
- Presentation — endpoints and request contracts, depends on Application

`src/API/FourPrep.Api` composes everything.

Forbidden:
- Domain depending on Infrastructure or on EF Core
- Application touching a `DbContext`
- business logic in Presentation
- persistence concerns leaking into Domain

Each module's `test/FourPrep.Modules.<Module>.ArchitectureTests` checks these layer rules.

### 3.2 Modules are boundaries

Modules in `src/Modules/`:

| Module | State |
|---|---|
| Auth | implemented (Keycloak accounts, login, sessions, own account) |
| Student | implemented (profiles, parent links, mentors) |
| Content | skeleton, next to build |
| Academy | skeleton |
| Payment, Admission, Agreement, AI, Chat, Notification, Storage | skeletons |

A skeleton has its four projects, `AssemblyReference`, `ICurrentUser`, `IUnitOfWork`, a `DbContext`
with its schema, registration in `Program.cs` and `MigrationExtensions`, and its two test projects.
Fill a skeleton in; do not create a parallel module.

A module must not reference another module's projects. `test/FourPrep.ArchitectureTests/Layers/ModuleTests.cs`
enforces this for the four layer assemblies of every module listed in `BaseTest.Modules`.

### 3.3 Cross-module calls go through adapter projects

When module A needs something from module B:

1. A declares what it needs in its Application layer:
   `FourPrep.Modules.A.Application/Abstractions/B/IBService.cs`
2. An adapter project `src/Modules/A/FourPrep.Modules.A.B/` implements it by sending B's own
   query or command through MediatR `ISender`. Only the adapter references B.
3. A's Infrastructure registers the adapter (`services.AddScoped<IBService, BService>()`).

Existing examples:
- `FourPrep.Modules.Auth.Student` — Auth creates the student profile at registration
  (`IStudentService` → `CreateStudentCommand`)
- `FourPrep.Modules.Student.Auth` — Student checks that a user has the `mentor` role
  (`IAuthService` → `GetUserRolesQuery`)

Never query another module's schema with SQL, and never share entities between modules.

---

## 4. Repository layout

```text
src/
  API/
    FourPrep.Api/            composition root: Program.cs, Extensions/, Middleware/, appsettings*.json
  Common/
    FourPrep.Common.Domain/
    FourPrep.Common.Application/
    FourPrep.Common.Infrastructure/
    FourPrep.Common.Presentation/
    tests/
      FourPrep.Common.Tests/ shared test helpers (ShouldBeSuccessful), referenced by every test project
  Modules/
    <Module>/
      FourPrep.Modules.<Module>.Domain/
      FourPrep.Modules.<Module>.Application/
      FourPrep.Modules.<Module>.Infrastructure/
      FourPrep.Modules.<Module>.Presentation/
      FourPrep.Modules.<Module>.<OtherModule>/   adapter, only when needed (see 3.3)
      test/
        FourPrep.Modules.<Module>.ArchitectureTests/
        FourPrep.Modules.<Module>.UnitTests/
test/
  FourPrep.ArchitectureTests/  solution-wide rules (module isolation, Domain and Infrastructure folders)
keycloak/realms/fourprep-realm.json   imported on Keycloak's first start
docker-compose.yml                    Keycloak + Mailpit
four-prep.slnx
```

---

## 5. Common is the shared kernel

Common holds cross-cutting building blocks and the third-party libraries behind them. Modules do not
bring in their own infrastructure patterns. Check Common before writing something generic in a module.

What Common has today:

| Area | Types |
|---|---|
| Result / errors | `Result`, `Result<T>`, `Error`, `ErrorType`, `ValidationError`, `FieldError`, `LocalizedMessage` |
| Messaging | `ICommand`, `ICommandHandler`, `IQuery`, `IQueryHandler`, `IPagedListQuery`, `IPagedListQueryHandler` |
| Pipeline | exception handling, request logging, validation behaviors |
| Paging | `PagingRequest<T>`, `PagedList<T>` |
| Data | `IDbConnectionFactory` (Dapper), `DateOnly` type handler |
| Time | `IDateTimeProvider`, `ITimeZoneContext`, `TimeZones` |
| Localization | `Countries`, `Locales`, `PhoneNumbers`, `LocalizationErrors` |
| Validation | `RuleBuilderExtensions`: `WithError`, `CountryCode`, `TimeZoneId`, `Locale`, `PhoneNumber` |
| Authorization | `Roles`, `Policies`, JWT + Keycloak role mapping |
| Presentation | `IEndpoint`, `AddEndpoints`/`MapEndpoints`, `ApiResults.ToResponse`, `RateLimitPolicies` |
| Shared enums | `Exam` |
| Exceptions | `ApplicationException` |

Good candidates for Common: cross-cutting abstractions, generic behaviors, shared validation rules,
auth primitives, enums more than one module needs (like `Exam`).

Bad candidates: anything one module owns — its errors, DTOs, repositories, commands and queries.

Do not add to Common "in case a module needs it later". Common was cleaned of tofan leftovers
(currencies, signed file links, Excel export, Uz/Ru-named localizable entities) because nothing used
them. Add a piece when the first module actually needs it.

---

## 6. Technology decisions

### 6.1 EF Core is for writes only
Inserts, updates, deletes and state changes go through EF Core, a repository and `IUnitOfWork`.
No EF Core for lists, search screens, reports or other reads.

### 6.2 Dapper is for reads only
Lists, details, lookups and reports use Dapper through `IDbConnectionFactory`. No writes through Dapper.

### 6.3 Repositories serve the write side
A repository interface sits in Domain, its implementation is `internal sealed` in Infrastructure.
It loads one aggregate for a write or checks existence (`GetAsync`, `GetByUserIdAsync`,
`GetByLinkCodeAsync`, `IsLinkCodeTakenAsync`) and adds new ones (`Insert`). It never returns lists
or read models; those are Dapper queries in Application.

### 6.4 Unit of Work for every write
Persist with `IUnitOfWork.SaveChangesAsync` once per handler. The module's `DbContext` implements
`IUnitOfWork`.

### 6.5 MediatR and CQRS
Every use case is a command or a query. The command and its handler are in one file; the query and
its handler are in one file. The validator is a sibling file, `<Command>Validator.cs`.

### 6.6 FluentValidation
Input validation lives in validators and runs through the validation behavior, not in endpoints or
handlers. Use `WithError(<Module>Errors.X)` so the client gets the same localized error as from a
handler, and the Common rules (`CountryCode()`, `TimeZoneId()`, `Locale()`, `PhoneNumber()`) for
those fields. Those rules let an empty value through; add `NotEmpty()` when the field is required.

### 6.7 Serilog
Structured logging only. No other logging approach.

### 6.8 JSON
The API uses the ASP.NET Core default (System.Text.Json). Newtonsoft.Json is only used where the code
talks to Keycloak's admin API and token payloads; keep it there.

### 6.9 Sonar
`SonarAnalyzer.CSharp` runs on every project (`Directory.Build.props`). Write code it accepts:
meaningful names, small methods, constants instead of magic values, no duplication, no dead code.

### 6.9.1 No comments
Do not write comments in C# code: no `//`, no `/* */`, no `///`. This is tofan's rule and FourPrep
follows it.

If code seems to need a comment, change the code: a better name, an extracted method, a named
constant.

Where the reasoning goes instead:

| What | Where |
|---|---|
| Why a change was made | the commit message |
| How a module works and why | the module's section in `README.md` |
| A decision with rejected alternatives | the commit message and `README.md` |

Generated files (EF migrations and their `// <auto-generated />` header) are the exception; never
edit them by hand.

### 6.10 Keycloak is the identity provider
Users live in Keycloak, not in a FourPrep table. The Auth module talks to Keycloak through
`IIdentityProviderClient`; other modules never call Keycloak. Login is username and password only.
Email verification and password reset use Keycloak's own emails (no SMS).

Profile values the platform needs on every request (`country`, `zoneinfo`, `locale`) are Keycloak
user attributes that reach the access token as claims. A new attribute must also be declared in the
realm's User Profile in `keycloak/realms/fourprep-realm.json`, otherwise Keycloak drops it silently.

---

## 7. Global users: never assume Uzbekistan

FourPrep's students are worldwide. This is the biggest difference from tofan.

- Country: ISO 3166-1 alpha-2 code (`Countries`), stored upper case.
- Time zone: IANA name (`TimeZones`); default is UTC.
- Locale: `en`, `uz` or `ru` (`Locales`); default is English.
- Phone: E.164 (`PhoneNumbers.Normalize`), optional.
- Region: free text (state, province, viloyat), not an enum.
- Contact by email, not SMS.

Never hard-code: `+998` phones, Tashkent time, UZS as the only currency, viloyat lists, Uzbek-only
enums, `Name`/`NameUz`/`NameRu` columns. Data that must be shown in several languages gets a design
that works for any locale, decided with the user.

---

## 8. Layer-by-layer rules

### 8.1 Domain
Contains entities, repository interfaces and `<Entity>Errors`.

One folder, one aggregate: every folder in a module's Domain project holds one entity, its
`<Entity>Errors` and its `I<Entity>Repository`, and nothing else. A folder with only errors
(e.g. `Authentication/AuthenticationErrors`) is fine. `test/FourPrep.ArchitectureTests/Domain/DomainFolderTests.cs`
enforces this.

What does not fit goes elsewhere:
- calculators, rule and constant classes, read records → Application, in the feature that uses them
- a child entity → its own folder, changed only through the root (`Student.UpdateProfile` replaces its `ExamGoal`s)
- a repository method returning a read model → a Dapper query in Application

Entity style (see `Student`):
- `sealed class`, `Guid Id`
- a static `Create(...)` factory
- explicit methods for each change (`UpdateProfile`, `AssignMentor`, `RemoveMentor`, `ChangeLinkCode`);
  handlers call these instead of setting properties
- time comes in as a `DateTime utcNow` parameter; entities never read a clock
- `CreatedOnUtc` / `UpdatedOnUtc` where the record changes over time

No EF Core, Dapper, endpoints, request models or framework code in Domain.

### 8.2 Application
Contains commands, queries, their handlers and validators, read DTOs, and the module's abstractions
under `Abstractions/` (`Authentication/ICurrentUser`, `Data/IUnitOfWork`, adapter interfaces,
module services like `LinkCodes/ILinkCodeGenerator`).

Write handler flow:
1. load the aggregate through the repository
2. return `<Module>Errors.NotFound` if it is missing
3. check business rules, call adapters if another module must answer
4. call the entity method or `Create`, `Insert` if new
5. `SaveChangesAsync` once
6. return `Result.Success(...)`

Read handler flow: open a connection from `IDbConnectionFactory`, run SQL shaped to the response,
return the DTO or `PagedList<T>`. When several queries share the SQL, put it in an `internal static`
reader class next to them (`StudentReader`, `StudentList`).

No `DbContext`, no HTTP types, no persistence configuration in Application.

### 8.3 Infrastructure
Contains the `DbContext`, `Database/Schemas.cs`, EF configurations, repositories, migrations,
`CurrentUser`, implementations of the module's other abstractions, and `<Module>Module.cs` with
`Add<Module>Module`.

Folders: an aggregate's folder holds only `<Entity>Configuration` and `<Entity>Repository`, named
like its Domain folder. Other implementations go in a folder named after what they do
(`LinkCodes/`, `Identity/`, `Authentication/`). `test/FourPrep.ArchitectureTests/Infrastructure/InfrastructureFolderTests.cs`
enforces this.

### 8.4 Presentation
One `internal sealed class <UseCase> : IEndpoint` per endpoint, request records under `Requests/`.

An endpoint:
- maps the route and binds the request
- sends one command or query through `ISender`
- returns `result.ToResponse()`
- declares `.RequireAuthorization(Policies.X)` (only public endpoints like login and register have none),
  `.RequireRateLimiting(RateLimitPolicies.Email)` when it makes the platform send an email,
  its `.Produces(...)` status codes, and `.WithTags("<Module> / <Audience>")`

No business logic, database access or validation rules in endpoints.

---

## 9. API host

`FourPrep.Api` bootstraps Serilog, Swagger, exception handling, CORS, auth, rate limiting, the
Hangfire dashboard, health checks, Common services and every module.

A new module, or a skeleton coming to life, must be present in:
- `moduleApplicationAssemblies` and `Add<Module>Module` in `Program.cs`
- `MigrationExtensions` (`ApplyMigration<<Module>DbContext>`)
- `BaseTest.Modules` in `test/FourPrep.ArchitectureTests`
- `four-prep.slnx`

No business logic in the host.

---

## 10. Reference implementation

**Student** is the reference module: aggregates with child entities, repositories, a write flow with
an adapter call, Dapper lists with paging and filters, role-protected endpoints, unit tests with
fakes. Copy its shape for a new module.

**Auth** is special: its users are in Keycloak, so most of its handlers call `IIdentityProviderClient`
instead of a repository. Use it as the reference only for identity work.

When porting from tofan, check the result against Student, not against tofan.

---

## 11. Result and errors

Return `Result` / `Result<T>` for every expected outcome. Throw only for truly unexpected failures,
infrastructure faults and fatal configuration problems; the pipeline and `GlobalExceptionHandler`
turn those into a 500 without leaking details (a PostgreSQL unique violation becomes a 409
`Conflict.DuplicateKey`).

Each aggregate has `<Entity>Errors` in its Domain folder. Every error has a stable code and a message
in all three languages:

```csharp
public static readonly Error NotFound = Error.NotFound(
    "Student.NotFound",
    en: "The specified student was not found.",
    uz: "Ko'rsatilgan o'quvchi topilmadi.",
    ru: "Указанный ученик не найден.");
```

The response carries `messages` with `en`, `uz` and `ru`; the client shows the user's language.
Write natural sentences in each language, not word-for-word translations.

Codes: `<Entity>.<Problem>` — `Student.NotFound`, `Student.LinkCodeNotFound`, `Student.NotAMentor`,
`ExamGoal.DuplicateExam`.

Types: `NotFound` (404), `Conflict` (409), `Problem` (400, a rule the input breaks),
`Forbidden` (403), `Failure` (500, the message is hidden from the client).

Never build ad hoc errors inside handlers.

---

## 12. Date and time

Never call `DateTime.Now`, `DateTime.UtcNow` or `DateTimeOffset.Now` in application code.

- `IDateTimeProvider.UtcNow` for timestamps
- `ITimeZoneContext.Today()` / `ToLocal(...)` for the signed-in user's calendar date (birthdays,
  "today's lesson", deadlines); it uses the `zoneinfo` claim and falls back to UTC

Store UTC. Convert to the user's zone only for date logic and display.

---

## 13. Current user and authorization

Each module has its own `ICurrentUser` in Application and `CurrentUser` in Infrastructure, built on
the Common `ClaimsPrincipal` helpers. Handlers never read claims directly.

Authorization is role based: `.RequireAuthorization(Policies.X)` on the endpoint. Policies are defined
in `AuthenticationExtensions`; a policy also lets in the roles above it (`Policies.Mentor` accepts
`mentor`, `mentor_admin` and `super_admin`), and `Policies.Staff` accepts every staff role. Add a new
policy there rather than checking roles in a handler. A rule that depends on data ("only this student's parent", "only the
assigned mentor") is checked in the handler and returns a module error.

---

## 14. File placement

| What | Where |
|---|---|
| Command + handler | `FourPrep.Modules.<M>.Application/<Entities>/<UseCase>/<UseCase>Command.cs` |
| Validator | same folder, `<UseCase>CommandValidator.cs` |
| Query + handler | `FourPrep.Modules.<M>.Application/<Entities>/<UseCase>/<UseCase>Query.cs` |
| Read DTO, filter | same folder as the query (`StudentResponse`, `StudentFilter`) |
| Entity, errors, repository interface | `FourPrep.Modules.<M>.Domain/<Entities>/` |
| Repository, EF configuration | `FourPrep.Modules.<M>.Infrastructure/<Entities>/` |
| Migration | `FourPrep.Modules.<M>.Infrastructure/Database/Migrations/` |
| Endpoint | `FourPrep.Modules.<M>.Presentation/<Entities>/<UseCase>.cs` |
| Request model | `FourPrep.Modules.<M>.Presentation/<Entities>/Requests/` |
| Adapter to another module | `src/Modules/<M>/FourPrep.Modules.<M>.<Other>/` |

---

## 15. Read side

- Dapper, parameterized SQL, never string-concatenated values
- alias columns with `nameof(Response.Property)`
- use the module's schema explicitly (`FROM student.students s`)
- paged lists take `PagingRequest<TResponse>`; its sortable columns are the response's properties in
  snake_case, so `ORDER BY s.{paging.SortBy} {paging.SortDirection}` is safe
- return DTOs, never entities

---

## 16. Database

- each module owns one PostgreSQL schema (`Database/Schemas.cs`) and its migrations history table
  lives in that schema
- `UseSnakeCaseNamingConvention()` on every `DbContext`
- migrations belong to Infrastructure and are applied at startup when
  `Database:ApplyMigrationsOnStartup` is true
- no table in one schema references a table in another; keep the other module's id as a plain column
  (`MentorUserId`, `UserId`)

---

## 17. Testing

Every meaningful change comes with tests unless the user says otherwise.

Unit tests (`FourPrep.Modules.<M>.UnitTests`): xUnit + FluentAssertions. Test validators, entity
behavior and handlers. Dependencies are hand-written fakes in the test project (`Fakes.cs`:
`FakeClock`, `FakeTimeZoneContext`, `FakeCurrentUser`, in-memory repositories); there is no mocking
library.

Architecture tests: per module (layer direction; sealed and non-public handlers, validators and
endpoints; `Command`/`Query`/`Handler`/`Validator` name suffixes; no EF Core in Domain or Application)
and solution-wide in
`test/FourPrep.ArchitectureTests`. They all use `ShouldBeSuccessful()` from `FourPrep.Common.Tests`.

Before finishing: `dotnet build four-prep.slnx` with no new warnings and `dotnet test four-prep.slnx`
green.

---

## 18. Naming

Names say the use case: `AssignMentorCommand`, `GetMyChildrenQuery`, `StudentSummaryResponse`,
`StudentParentErrors`, `LinkCodeGenerator`. Endpoints are named like the use case (`AssignMentor`).
"My" marks the signed-in user's own data (`GetMyStudent`, `UpdateMyAccount`).

Avoid `Service`, `Manager`, `Helper`, `Util`, `DataModel`. The one accepted `Service` suffix is the
cross-module adapter (`IStudentService`, `IAuthService`).

---

## 19. How to approach a request

1. Which module owns it? Is a cross-module call needed (then an adapter)?
2. Read side or write side?
3. Which layer owns each part?
4. What does Student already do that looks like this?
5. What in Common should be reused?
6. Which errors (in en, uz, ru) and validation rules are needed?
7. Does it work for a user in any country, time zone and language?
8. What changes in DI, migrations, endpoints, policies, the Keycloak realm, `README.md`?
9. Which tests?

A "quick fix" still goes in the right module, layer and abstraction.

---

## 20. Forbidden

- EF Core for reads, Dapper for writes
- business logic in endpoints, `DbContext` in Application
- repository interfaces outside Domain, implementations outside Infrastructure
- one module referencing another module's projects, or querying its schema
- a module duplicating something Common already has
- exceptions for expected business outcomes, ad hoc errors, an error without `en`/`uz`/`ru`
- `DateTime.UtcNow` and friends in application code
- reading claims or calling Keycloak outside Auth and the Common auth helpers
- Uzbekistan-only assumptions (section 7)
- copying tofan code without removing what FourPrep does not use
- comments of any kind (section 6.9.1)

---

## 21. Final instruction

Do not invent a second architecture inside this repository. Optimize for consistency, modularity,
readiness for service extraction, and a platform that works for a student anywhere in the world.
When in doubt, mirror the Student module.
