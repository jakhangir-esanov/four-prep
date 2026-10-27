---
paths:
  - "src/Modules/*/FourPrep.Modules.*.Presentation/**/*.cs"
  - "src/Common/FourPrep.Common.Presentation/**/*.cs"
---

# Endpoint rules

- One `internal sealed class <UseCase> : IEndpoint` per endpoint, in `<Entities>/<UseCase>.cs`.
  Request records in `<Entities>/Requests/<UseCase>Request.cs`, `internal sealed record` with
  `{ get; init; }` properties.
- The handler lambda binds the route and body, builds one command or query, sends it through
  `ISender` and returns `result.ToResponse()`. No business logic, no database, no validation, no
  try/catch.
- Authorization:
  - `.RequireAuthorization(Policies.X)` for anything a role owns. Policies let in the roles above
    them (`Policies.Mentor` admits `mentor_admin` and `super_admin`).
  - `.RequireAuthorization()` only for "my account" endpoints every signed-in user may call.
  - No `RequireAuthorization` only for login, register, refresh, forgot-password and Google sign-in.
  - A policy checks the role, not the relationship. "This parent's child", "this mentor's student"
    is checked in the handler.
- `.RequireRateLimiting(RateLimitPolicies.Email)` on every endpoint that makes the platform send an
  email.
- Declare every status the use case can return: `.Produces<Result<T>>(200)`, `.Produces<Result>(400)`,
  `401` and `403` when authorized, `404`/`409` when the handler has such errors,
  `.Produces(500)`.
- `.WithTags("<Module> / <Audience>")`, e.g. `"Student / Admin"`, `"Auth / Authentication"`.
- Routes: plural resources, `me` for the signed-in user (`students/me`, `parents/me/students`),
  `admin/...` for staff management, typed route constraints (`{id:guid}`).
- A response never carries more than the caller's screen needs. A student's personal data and link
  code are for the student, linked parents, the assigned mentor and admins only
  (`.claude/rules/identity-security.md`).
