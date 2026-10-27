---
name: new-module
description: Brings a FourPrep skeleton module to life (Content, Academy, Payment, Admission, Agreement, AI, Chat, Notification, Storage) — its first aggregate, persistence, migration, use cases and endpoints — or adds a module that does not exist yet. Use whenever the user says "yangi modul", "modulni boshlaymiz", "Content'ni qilamiz", "new module", or starts work on a module that has no aggregate yet.
---

# New module

## Inputs to confirm

- Module name (PascalCase, as in `src/Modules`: `Content`)
- The first aggregate (singular PascalCase: `Course`) and what the user does with it
- Who uses it: which roles read it, which roles change it (`Roles` / `Policies`)
- Whether it needs another module (then an adapter, CLAUDE.md 3.3)

If the user already gave them, do not ask again. Ask about business rules you cannot infer; do not
invent fields, limits or exam lists (the `Exam` values are still unconfirmed).

Before creating anything new, check `src/Modules`: Content, Academy, Payment, Admission, Agreement, AI,
Chat, Notification and Storage already exist as skeletons. Fill the skeleton; never create a parallel
module. If tofan has the same module, port its shape, then remove everything FourPrep does not use and
everything Uzbekistan-only.

## A skeleton already has

- the four projects and both test projects, `AssemblyReference`s
- `Abstractions/Authentication/ICurrentUser`, `Abstractions/Data/IUnitOfWork`
- `Infrastructure/Authentication/CurrentUser`, `Database/<M>DbContext` (implements `IUnitOfWork`),
  `Database/Schemas.cs`, `<M>Module.cs` with the DbContext, `CurrentUser` and `IUnitOfWork` registered
- registration in `Program.cs`, `MigrationExtensions`, `BaseTest.Modules`, `four-prep.slnx`

## Steps

1. Read the Student module end to end; it is the reference. Copy its shape, not its content.
2. **Domain** — `<Entities>/<Entity>.cs`, `<Entity>Errors.cs` (en, uz, ru),
   `I<Entity>Repository.cs` (`.claude/rules/domain.md`).
3. **Infrastructure** — `<Entities>/<Entity>Configuration.cs` and `<Entity>Repository.cs`;
   `DbSet` and `ApplyConfiguration` in `<M>DbContext`; repository registered in `<M>Module.cs`
   (`.claude/rules/infrastructure.md`).
4. **Migration** — `dotnet tool restore`, then
   `dotnet ef migrations add InitialMigration --project src/Modules/<M>/FourPrep.Modules.<M>.Infrastructure --startup-project src/API/FourPrep.Api --context <M>DbContext --output-dir Database/Migrations`.
   Read the generated migration: schema `<m>`, snake_case, the indexes you expect, no table from
   another module.
5. **Use cases** — for each one, follow the `new-use-case` skill: command or query, validator,
   endpoint, tests.
6. **Policies** — if a new role or policy is needed, add it to `Roles`/`Policies`, to
   `AuthenticationExtensions`, and to the realm file (`.claude/rules/keycloak.md`). Ask first: a new
   role is a product decision.
7. **Cross-module** — `Abstractions/<Other>/I<Other>Service` here, an adapter project
   `FourPrep.Modules.<M>.<Other>` implementing it through `ISender`, registered in `<M>Module.cs`,
   referenced by the Infrastructure project and added to `four-prep.slnx`.
8. **README** — a section for the module in Uzbek: what it does, who uses which endpoints, rules the
   code cannot show.
9. **Verify** — `dotnet build four-prep.slnx -nologo -v q` with no new warnings,
   `dotnet test four-prep.slnx -nologo -v q`, then run the API and call the new endpoints
   (`/run` or the `fourprep-api` launch configuration).
10. Delegate to `architecture-reviewer`, and to `security-reviewer` if the module reads or returns
    people's data. Fix blocking items.

## Do not

- Reference another module's projects or query its schema.
- Add to Common something only this module uses.
- Use EF Core for reads or Dapper for writes.
- Hard-code anything that assumes Uzbekistan (`+998`, Tashkent time, UZS, viloyat lists, `NameUz`).
- Create endpoints before the user asks for the use case.
