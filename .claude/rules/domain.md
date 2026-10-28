---
paths:
  - "src/Modules/*/FourPrep.Modules.*.Domain/**/*.cs"
  - "src/Common/FourPrep.Common.Domain/**/*.cs"
---

# Domain rules

- One folder, one aggregate: `<Entities>/` holds `<Entity>.cs`, `<Entity>Errors.cs`,
  `I<Entity>Repository.cs` and enums only that aggregate uses (`ClassGroupStatus`), nothing else.
  A folder with only errors is fine (`Authentication/AuthenticationErrors`). `test/FourPrep.ArchitectureTests/Domain/DomainFolderTests.cs`
  enforces it.
- Entities are `sealed`, have `Guid Id`, a static `Create(...)` and one method per change
  (`UpdateProfile`, `AssignMentor`, `ChangeLinkCode`). Handlers call those methods; they never set
  properties.
- Time comes in as `DateTime utcNow`. An entity never reads a clock and never calls `IDateTimeProvider`.
- A child entity gets its own folder and is changed only through its root
  (`Student.UpdateProfile` replaces its `ExamGoal`s).
- Another module's record is referenced by id only (`UserId`, `MentorUserId`), never by type.
- No EF Core, Dapper, ASP.NET, MediatR or FluentValidation. No request models, read models or
  calculators; those go to Application next to the feature that uses them.

## Repository interfaces

- Write side only: load one aggregate (`GetAsync`, `GetByUserIdAsync`, `GetByLinkCodeAsync`), check
  existence (`IsLinkCodeTakenAsync`), `Insert`, and `Remove` where the use case deletes.
- Never a list, search or projection method. Lists are Dapper queries in Application.

## Errors

```csharp
public static readonly Error NotFound = Error.NotFound(
    "Student.NotFound",
    en: "The specified student was not found.",
    uz: "Ko'rsatilgan o'quvchi topilmadi.",
    ru: "Указанный ученик не найден.");
```

- Code: `<Entity>.<Problem>`, stable once released; the client switches on it.
- Every error has natural `en`, `uz` and `ru` sentences. Not word-for-word translations, and nothing
  that only makes sense in Uzbekistan.
- Type decides the status: `NotFound` 404, `Conflict` 409, `Problem` 400 (the input breaks a rule),
  `Forbidden` 403, `Failure` 500 (message hidden from the client).
- An error that more than one aggregate of a module uses stays with the aggregate that owns the rule.

## Common.Domain

Only what several modules need: `Result`/`Error`, `Roles`, `Policies`, `Countries`, `Locales`,
`PhoneNumbers`, shared enums such as `Exam`. Add to it when a second module needs something, not before.
