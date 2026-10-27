---
paths:
  - "src/Modules/*/test/**/*.cs"
  - "src/Common/tests/**/*.cs"
  - "test/**/*.cs"
  - "keycloak/fourprep-keycloak-extensions/src/test/**/*.java"
---

# Test rules

- xUnit with `Assert`. FluentAssertions only in architecture tests through `ShouldBeSuccessful()`.
  No mocking library: dependencies are hand-written fakes in the module's `Fakes.cs`
  (`FakeClock`, `FakeTimeZoneContext`, `FakeCurrentUser`, `FakeUnitOfWork`, in-memory repositories,
  fake adapters). Extend a fake rather than adding a second one.
- Arrange / Act / Assert, one behaviour per test. Names read as a sentence:
  `Handle_DeletesTheAccountWhenTheStudentProfileFails`, `Validate_WhenTheUsernameIsEmpty_Fails`.
- The test folder mirrors Application (`Students/`, `StudentParents/`, `Authentication/`).
- Time is fixed through the fakes. Never the real clock, never a real wait.
- A failure test asserts the exact error (`Assert.Equal(StudentErrors.NotFound, result.Error)`) and
  what did not happen (`SaveCount == 0`, nothing inserted, no session, the account deleted).
- Validator tests assert `ErrorCode` and `PropertyName`, not message text.
- Every bug fix starts with a failing test.
- No comments in tests.

## Required coverage

A review rejects a change that lands without these.

**Every use case**: the happy path, each `Result.Failure` branch, and for writes that nothing is saved
on failure.

**Every validator**: a valid command passes; each rule fails on a bad value with its code.

**Security** (Auth, Student and any endpoint returning people's data):

- A "my" query returns only the current user's records, given records of other users in the fake.
- A by-id write by a parent, mentor or student on someone else's record returns the module error and
  changes nothing.
- Link codes: an unknown code fails without linking; the 5th parent is refused; a regenerated code
  replaces the old one.
- Sign-up grants only `student` or `parent`; a staff role from a public request fails validation.
- Google sign-in: a staff account, or a user who also holds a staff role, gets
  `GoogleSignInNotAllowed` and its Keycloak session is ended; a new account without role and country
  gets `SignUpDetailsRequired`.

**Architecture**: a new module is added to `BaseTest.Modules`, and the module's own architecture test
project builds against its four layers.

**Keycloak extension**: pure rules (token claims, allowed roles) have JUnit tests; behaviour that needs
Keycloak is proven on a throwaway container (`.claude/rules/keycloak.md`).

## Running

- One module: `dotnet test src/Modules/<M>/test/FourPrep.Modules.<M>.UnitTests -nologo -v q`
- Everything: `dotnet build four-prep.slnx -nologo -v q` (no new warnings) and
  `dotnet test four-prep.slnx -nologo -v q`
- Extension: `docker build -t fourprep-keycloak:dev keycloak`
