---
name: new-use-case
description: Adds one FourPrep use case end to end — a command or query with its handler, validator, endpoint, request, errors, persistence changes and tests — inside an existing module. Use whenever the user asks for a new endpoint, API, action or screen data ("yangi endpoint", "API qo'sh", "ro'yxatini chiqar", "o'zgartirish imkoni"), or /endpoint.
---

# New use case

Every use case is one command or one query, owned by one module, exposed by one endpoint. Decide the
shape before writing code.

## Before writing code

1. Which module owns the behaviour? If it needs another module, an adapter call is part of the plan.
2. Read side or write side? Reads are Dapper queries; writes go through an entity method, a
   repository and `SaveChangesAsync`.
3. Who calls it: which policy, and which relationship must the handler check (my own record, my child,
   my mentee)?
4. What are the failure cases, and which `<Entity>Errors` do they need (en, uz, ru)?
5. Does it work for a user in any country, time zone and language?
6. What does the response contain, and what must it not contain (`.claude/rules/identity-security.md`)?

## Build order

1. **Domain** — the entity method (taking `DateTime utcNow`) and new errors. A new repository method
   only if the write must load the aggregate another way.
2. **Application** —
   - write: `<Entities>/<UseCase>/<UseCase>Command.cs` (record + handler) and
     `<UseCase>CommandValidator.cs`
   - read: `<UseCase>Query.cs` (record + handler), the response record, a filter record if the list is
     filtered, SQL in a shared reader if another query uses it
   (`.claude/rules/application.md`, `.claude/rules/validation.md`)
3. **Infrastructure** — configuration and migration if the schema changes, repository method if added,
   registration if a new service appears (`.claude/rules/infrastructure.md`).
4. **Presentation** — `<Entities>/<UseCase>.cs` and `Requests/<UseCase>Request.cs` with policy,
   `Produces` and tags (`.claude/rules/endpoints.md`).
5. **Tests** — handler and validator tests in the module's UnitTests project, including the required
   security cases (`.claude/rules/testing.md`). Use the `test-writer` agent if the list is long.
6. **Docs** — the README section of the module when the behaviour is something a frontend developer
   needs to know (a new flow, an error to handle, a limit).
7. **Verify** — `dotnet build four-prep.slnx -nologo -v q` with no new warnings,
   `dotnet test four-prep.slnx -nologo -v q`, then call the endpoint on the running API with a token
   of each role that should and should not get through.
8. **Review** — `architecture-reviewer`; `security-reviewer` when the use case touches Auth, Student,
   Keycloak or anyone's personal data.

## Quality bar

- The endpoint is thin and the handler reads as the use case: load, check, change, save, return.
- Every failure the client can act on has its own error code; nothing falls into a generic 500.
- A list is paged and sortable through `PagingRequest<T>` unless it is bounded by design.
- A "my" endpoint cannot be turned into someone else's by changing an id.
- No comments; names carry the intent.
