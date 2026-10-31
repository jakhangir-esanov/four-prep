---
paths:
  - "src/Modules/Auth/**/*.cs"
  - "src/Modules/Student/**/*.cs"
  - "src/Common/FourPrep.Common.Infrastructure/Authentication/**/*.cs"
  - "src/Common/FourPrep.Common.Domain/Authorization/**/*.cs"
  - "keycloak/**"
---

# Identity and student-data security rules

## Threat model

Most users are minors, from any country. Their profile (name, date of birth, school, grade, country,
exam goals) is personal data of a child. Accounts live in Keycloak; FourPrep tables hold profiles and
links keyed by the Keycloak user id.

Real risks, in order:

1. **Someone reads a child's profile they have no relationship to.** A by-id endpoint guarded only by
   a role, a list that returns more than its screen needs, a parent response that includes another
   parent's data.
2. **A stranger attaches themself as a parent.** The 8-character link code is a bearer secret: whoever
   holds it can link, up to 4 parents per student.
3. **A staff account is taken over or exposed through the user panel.** Google sign-in is for
   `student` and `parent` only; staff sign in to the staff panel with username and password.
4. **Account enumeration and email abuse** through public endpoints.

## Rules

- Roles come from Keycloak realm roles in the token; policies in `AuthenticationExtensions` decide by
  role. A relationship (my child, my mentee, my own profile) is checked in the handler against
  `ICurrentUser.UserId`, and the query is scoped to it in SQL.
- Who may see a student's profile: the student, linked parents, the assigned mentor, `mentor_admin`,
  `super_admin`. Nobody else, including other parents of other children and teachers.
- The link code is shown to the student only (`GET /students/me`). Never put it in a parent, mentor or
  list response, a log, or an error message. Regenerating it must invalidate the old one.
- `POST /parents/me/students` allows 10 attempts per hour per IP (`RateLimitPolicies.LinkCode`),
  because a linked parent reads the child's profile and whole chat history. Any change that makes codes
  easier to guess (shorter, fewer characters, reusable) tightens it first.
- Self-service sign-up (`/auth/register`, `/auth/register-parent`, `/auth/google`) grants only
  `student` or `parent`. Staff are created by `super_admin` through `POST /admin/users`.
- Google sign-in:
  - The authenticator's `allowedRoles` (`student##parent`) refuses any account holding another role
    before it is linked or claimed; `GoogleLoginCommand` checks again. Keep both.
  - Claiming an account whose email is unverified (passwords removed, sessions ended) is the agreed
    policy against email squatting; do not weaken it.
  - Keep `validateSignature` on and the `iss`, `aud`, `sub`, `exp` checks in the extension.
- Public endpoints do not reveal whether an account exists: failed logins share
  `InvalidCredentials`, Google failures share `InvalidGoogleToken` (the staff 403 is only reachable by
  whoever controls that verified email), `forgot-password` always answers 200.
- Every endpoint that sends an email has `RequireRateLimiting(RateLimitPolicies.Email)`.
- Tokens, passwords, link codes and client secrets never reach a log, an error message, a query string
  or a response they do not belong to. `UserSession` stores only the SHA-256 of the access token.
- `fourprep-dev-secret`, `superadmin`/`superadmin` and `admin`/`admin` are local-only values.
- Ask before adding anything that sends student data off-platform (analytics, AI providers, email
  content beyond Keycloak's own messages).
