---
paths:
  - "src/Modules/*/FourPrep.Modules.*.Application/**/*Validator.cs"
  - "src/Common/FourPrep.Common.Application/Extensions/RuleBuilderExtensions.cs"
---

# Validation rules (FluentValidation)

- One `internal sealed class <UseCase>CommandValidator : AbstractValidator<<UseCase>Command>` in its own
  file next to the command. The validation pipeline behavior runs it; endpoints and handlers never
  validate input themselves.
- A rule the client must recognise ends with `.WithError(<Entity>Errors.X)` so it returns the same
  code and en/uz/ru messages a handler would. Plain length and emptiness checks may keep
  FluentValidation's default messages.
- International fields use the Common rules, never a local regex:
  `CountryCode()` (ISO 3166-1 alpha-2), `TimeZoneId()` (IANA), `Locale()` (`en`, `uz`, `ru`),
  `PhoneNumber()` (E.164 after `PhoneNumbers.Normalize`). They accept an empty value; add
  `NotEmpty()` when the field is required.
- Constants instead of magic numbers (`MaxGrade`, `MaxExamGoals`), declared in the validator unless the
  domain owns the limit.
- A rule that needs the user's date takes `ITimeZoneContext` in the constructor and uses `Today()`.
- Roles a caller may choose are listed explicitly (`role is Roles.Student or Roles.Parent`); never
  accept any role from a public endpoint.
- Rules that need the database or another module (is the code taken, is the user a mentor) are business
  rules: they go in the handler and return a module error, not in the validator.
- Every validator has tests: one passing command, and for each rule a failing value with its
  `ErrorCode` and `PropertyName` asserted.
