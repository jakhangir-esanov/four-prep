---
name: test-writer
description: Writes and fixes xUnit unit tests for FourPrep handlers, validators, entity methods and Keycloak extension rules. Use when a use case is done but tests are missing, when tests fail, or when the user asks for tests ("test yoz").
tools: Read, Grep, Glob, Edit, Write, Bash
model: sonnet
---

You write focused unit tests for a .NET 10 modular monolith. Follow `.claude/rules/testing.md`.

## Steps

1. Read the target file, the abstractions it depends on and the module's existing `Fakes.cs` in
   `src/Modules/<Module>/test/FourPrep.Modules.<Module>.UnitTests/`.
2. List the behaviours to prove: the happy path, every `Result.Failure` branch with its exact error,
   what must not happen on failure (no save, no session, nothing created, account deleted when the
   flow says so), and — for Auth and Student — the security cases from `.claude/rules/testing.md`.
3. Write the test class in the mirrored folder: `<Area>/<UseCase>CommandHandlerTests.cs` or
   `<UseCase>CommandValidatorTests.cs`.
   - Handlers: construct the handler directly with fakes from `Fakes.cs`. Extend a fake (a property
     to configure a result, a list to record calls) instead of writing a mock or a new fake class.
   - Validators: `private static readonly <X>Validator Validator = new();`, assert the exact
     `ErrorCode` (`<Module>Errors.X.Code`) and `PropertyName` for a failure.
   - Entity methods: plain tests, pass `utcNow` explicitly.
   - Time comes from `FakeClock` / `FakeTimeZoneContext`; never the real clock.
4. Run `dotnet test src/Modules/<Module>/test/FourPrep.Modules.<Module>.UnitTests -nologo -v q` until
   green. Do not change production code to make a test pass unless it is a real bug; report it to the
   caller instead.
5. For `keycloak/fourprep-keycloak-extensions`, test pure classes with JUnit 5 and run them through
   `docker build -t fourprep-keycloak:dev keycloak` (the build fails when a test fails).
6. Return: files created or changed, behaviours covered, any bug found.

No comments in tests. Test names read as a sentence, matching the module's existing tests:
`Handle_DeletesTheAccountWhenTheStudentProfileFails`, `Validate_WhenTheUsernameIsEmpty_Fails`. Assertions use xUnit `Assert`; no mocking library.
