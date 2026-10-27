---
paths:
  - "src/Modules/*/FourPrep.Modules.*.Application/**/*.cs"
  - "src/Common/FourPrep.Common.Application/**/*.cs"
---

# Application rules

## Commands (write side)

- `<Entities>/<UseCase>/<UseCase>Command.cs` holds `public sealed record <UseCase>Command(...) : ICommand`
  (or `ICommand<T>`) and `internal sealed class <UseCase>CommandHandler(...)` in the same file.
- Flow: load the aggregate through its repository → return `<Entity>Errors.NotFound` if missing →
  check rules, call adapters → call the entity method or `Create` + `Insert` → `SaveChangesAsync` once →
  `Result.Success(...)`.
- Every expected outcome is a `Result`. Throw only for what the pipeline should turn into a 500.
- Time: `IDateTimeProvider.UtcNow` for timestamps, `ITimeZoneContext.Today()` for the user's own date.
- Current user: the module's `ICurrentUser`. Never claims, never `HttpContext`.
- When a later step fails after an external side effect (a Keycloak account, a profile in another
  module), undo it the way `AccountRoles.GrantAsync` deletes the account, and test that path.

## Queries (read side)

- `<UseCase>Query.cs` holds the query record and `internal sealed class <UseCase>QueryHandler`.
- Dapper through `IDbConnectionFactory`; no EF Core, no repositories, no entities in the result.
- SQL names the schema (`FROM student.students s`), aliases columns with `nameof(Response.Property)`,
  and passes every value as a parameter. Only `PagingRequest<T>.SortBy` / `SortDirection` may be
  interpolated, because they are checked against the response's snake_case properties.
- Paged lists: `IPagedListQuery<TResponse>` + `PagingRequest<TResponse>` → `PagedList<TResponse>`.
- SQL shared by several queries goes in an `internal static` reader next to them (`StudentReader`,
  `StudentList`).
- "My" queries filter by `ICurrentUser.UserId` in SQL; never load everything and filter in memory.

## Abstractions

- `Abstractions/Authentication/ICurrentUser`, `Abstractions/Data/IUnitOfWork`, and one folder per
  outside dependency (`Abstractions/Identity/IIdentityProviderClient`, `Abstractions/LinkCodes/...`).
- Another module is reached only through `Abstractions/<Other>/I<Other>Service`, implemented in the
  adapter project `FourPrep.Modules.<Module>.<Other>` by sending that module's command or query.
  Never reference another module's Application, Domain or schema from here.

## Do not

- No `DbContext`, `HttpContext`, `IResult` or request DTOs.
- No `Service`, `Manager` or `Helper` classes for what a handler should do. The one accepted
  `Service` suffix is the cross-module adapter interface.
- No comments; a name or an extracted private method says it instead.
