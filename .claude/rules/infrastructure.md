---
paths:
  - "src/Modules/*/FourPrep.Modules.*.Infrastructure/**/*.cs"
  - "src/Modules/*/FourPrep.Modules.*.*/*Service.cs"
  - "src/Common/FourPrep.Common.Infrastructure/**/*.cs"
---

# Infrastructure rules

## Folders

- `<Entities>/` mirrors the Domain folder and holds only `<Entity>Configuration.cs` and
  `<Entity>Repository.cs`. `test/FourPrep.ArchitectureTests/Infrastructure/InfrastructureFolderTests.cs`
  enforces it.
- Anything else goes in a folder named after what it does: `Identity/`, `LinkCodes/`, `Authentication/`.
- `Database/` holds `<Module>DbContext`, `Schemas.cs` and `Migrations/`.
- `<Module>Module.cs` is the module's only registration entry point.

## Persistence

- Repositories are `internal sealed`, take the `DbContext` and load with `SingleOrDefaultAsync`; they
  never call `SaveChangesAsync` (the handler does, through `IUnitOfWork`).
- Every configuration is applied in `<Module>DbContext.OnModelCreating` with `ApplyConfiguration`.
- Snake_case comes from `UseSnakeCaseNamingConvention()`; do not name columns by hand unless the
  convention cannot.
- Strings get a `HasMaxLength` that matches the validator. Enums are stored as int. Ids of other
  modules' records are plain columns with an index where they are looked up, never a foreign key
  across schemas.
- Migrations: `dotnet tool restore`, then
  `dotnet ef migrations add <PascalCaseName> --project src/Modules/<M>/FourPrep.Modules.<M>.Infrastructure --startup-project src/API/FourPrep.Api --context <M>DbContext --output-dir Database/Migrations`.
  Name by what changes (`InternationalStudentLocation`). Never edit a generated file except to add
  data moves the change needs, and never edit a migration that is already committed.

## Registration

- `services.AddScoped<I<Entity>Repository, <Entity>Repository>()` for every repository,
  `IUnitOfWork` resolved from the `DbContext`, the module's `CurrentUser`, adapters
  (`services.AddScoped<IAuthService, AuthService>()`), stateless services as singletons.
- Options classes bind a section with a `SectionName` constant; secrets come from configuration, never
  from code.

## External systems

- Only the Auth module talks to Keycloak, through `KeycloakAdminClient`. It maps Keycloak answers to
  module errors (`UsernameTaken`, `GoogleSignInNotAllowed`) by stable strings kept as constants.
- Log the status and Keycloak's error body when a call fails; never log tokens, passwords, link codes
  or request bodies that carry them.

## Adapter projects (`FourPrep.Modules.<A>.<B>`)

- One class implementing `<A>`'s `I<B>Service` by sending `<B>`'s own command or query through
  `ISender`. No logic beyond translating the call and its result.
