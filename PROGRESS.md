# Replyo — Progress Log

A daily log of what got built, what got stuck, and what's next.

---

## Day 1 — May 2, 2026

**Done:**
- Verified development environment (Git 2.45, .NET 10.0.200, Node 24.12, Docker 29.4)
- Set up VS Code with C# Dev Kit, Tailwind IntelliSense, GitLens, Docker, Thunder Client
- Created project folder structure (backend, frontend, widget, docs, scripts)
- Initialized local git repository
- Wrote .gitignore for .NET, Node, IDE, and OS artifacts
- Created docker-compose.yml with Postgres 16 + pgvector and Redis 7
- Verified Postgres and pgvector extension working in container
- Scaffolded .NET 10 solution with clean architecture (Domain, Application, Infrastructure, Api + tests)
- Confirmed backend builds and runs (OpenAPI endpoint responding)
- Scaffolded React + Vite + TypeScript frontend
- Installed TanStack Query, React Router, Axios, Tailwind CSS v3
- Confirmed frontend dev server runs and renders Vite landing page
- Made 7 commits with conventional commit messages

**Stuck / resolved:**
- Windows Defender blocked initial `dotnet run`; resolved by adding `C:\personal-projects` to exclusions
- Tailwind v4 has removed `init` command and breaks shadcn/ui workflow; downgraded to v3 deliberately
- Git author email was auto-generated from machine name; fixed via `git config` and rewrote existing commit history with `git filter-branch`

**Decisions:**
- Targeting .NET 10 (current LTS) instead of .NET 8 — fully compatible with all chosen libraries
- Tailwind CSS v3 instead of v4 — better shadcn/ui ecosystem support
- Working entirely locally for now; GitHub remote will come later

---

## Day 2 — May 3, 2026

**Done:**

*Configuration & dependencies*
- Installed NuGet packages across the right layers: EF Core, Npgsql, Pgvector, FluentValidation, Hangfire, Serilog, Swashbuckle
- Pinned all EF Core packages to version 10.0.7 to resolve transitive version conflict
- Configured `appsettings.json` with full structure (Logging, ConnectionStrings, OpenAI, Cors)
- Created gitignored `appsettings.Development.json` with local Postgres + Redis connection strings
- Created committed `appsettings.Development.json.example` as a template for setup
- Updated README with proper "Getting started" section
- Made 7 commits with conventional commit messages

*Domain layer*
- `EntityBase` abstract class with Id (Guid), CreatedAt, UpdatedAt, Touch() helper
- Enums with explicit integer values: TenantStatus, KnowledgeDocumentStatus, MessageRole, KnowledgeDocumentSource
- 7 entities, all using private setters and static factory methods:
  - `Tenant` — multi-tenancy unit with branding and system prompt
  - `User` — login user scoped to one tenant
  - `KnowledgeDocument` — three source types via factory methods (upload, URL, text)
  - `KnowledgeChunk` — content + vector embedding for RAG, immutable
  - `WidgetVisitor` — anonymous end-user identified by session ID
  - `Conversation` — chat with escalation support
  - `Message` — immutable, role-specific factory methods (FromVisitor, FromAssistant, FromHumanAgent)

*EF Core configurations*
- Created `ICurrentTenant` interface in Application layer for multi-tenancy
- Built `ReplyoDbContext` with all 7 DbSets and pgvector extension declared
- Wrote individual `IEntityTypeConfiguration<T>` for every entity with snake_case columns, indexes, and FK behaviors that reflect business meaning (Cascade for child data, Restrict for users, SetNull for soft links)
- Vector embedding column configured as `vector(1536)` with float[] ↔ Vector type conversion
- Wired EF Core into DI in Program.cs with UseNpgsql + UseVector + dev-only sensitive logging

*Migration & verification*
- Generated `InitialCreate` migration cleanly
- Reviewed migration file before applying — confirmed embedding column type, all 7 tables, FK behaviors
- Applied migration to local Postgres successfully
- Verified all 8 tables (7 entity tables + `__EFMigrationsHistory`) exist
- Confirmed pgvector 0.8.2 is loaded and embedding column is `vector(1536)`

*Health checks*
- Added `AspNetCore.HealthChecks.NpgSql` and `AspNetCore.HealthChecks.Redis` to the API project
- Added Redis connection string to `appsettings.Development.json` and the committed `.example` template
- Wired up tagged health checks in `Program.cs` — postgres and redis both tagged `ready`
- Mapped two endpoints: `/health/live` (no checks, pure liveness) and `/health/ready` (runs all `ready`-tagged checks)
- Custom JSON response writer exposes per-check status, duration, description, and error message
- Verified both endpoints respond correctly: liveness returns 200 with checks empty, readiness returns 200 with both checks Healthy when Docker is up, 503 when dependencies are down

*OpenAPI / Scalar*
- Removed `Swashbuckle.AspNetCore` (was scaffolded by template, never used in code)
- Added `Scalar.AspNetCore` for the API explorer UI
- Gated both `MapOpenApi` and `MapScalarApiReference` to the Development environment to avoid leaking API surface in production
- Configured Scalar with C# / `HttpClient` as the default code sample target
- Verified `/openapi/v1.json` serves a valid document and `/scalar/v1` renders the UI

*Tests*
- Confirmed test projects already had everything needed: xUnit 2.9.3, FluentAssertions 6.12.2, `Microsoft.AspNetCore.Mvc.Testing` 10.0.7
- Deleted leftover `UnitTest1.cs` scaffolding from `Replyo.Application.Tests`
- Added `TenantTests.Create_WithValidInputs_SetsExpectedFields` — first domain unit test, proves the unit harness works
- Added `HealthEndpointTests.GetLiveness_ReturnsHealthy` — first integration test using `WebApplicationFactory<Program>`, proves the integration harness works
- Added `public partial class Program;` to `Program.cs` so the test project can reference the entry point as a generic argument
- Full suite green: 2 tests, 2 passing


**Stuck / resolved:**
- EF Core version conflict between 10.0.4 (transitive via Pgvector.EntityFrameworkCore) and 10.0.7 (direct references); resolved by pinning `Microsoft.EntityFrameworkCore.Relational` and other EF packages explicitly to 10.0.7 across all projects
- README markdown rendering issues with code blocks inside numbered lists; rewrote section using bold headings + standalone code fences
- `dotnet run` threw `ReflectionTypeLoadException` after adding health check packages because Swashbuckle 7.3.2 was compiled against Microsoft.OpenApi 1.x but .NET 10's built-in OpenAPI stack pulls in 2.x. Resolved by removing Swashbuckle entirely rather than upgrading — it was scaffolding cruft, never used in code, and Microsoft now explicitly recommends against pairing Swashbuckle with `WithOpenApi()` / `MapOpenApi()`
- Initial readiness check returned 503 because Docker Desktop wasn't running. Confirmed the readiness endpoint is doing real work, not just rubber-stamping. Started Docker, both checks went green


**Decisions:**
- Skipped pgvector index (HNSW/IVFFlat) for now — best added in a separate migration in Week 2 once we have real data and can tune it properly
- Left embedding value comparer warning for later — not blocking since chunks are immutable in our design
- Liveness / readiness split instead of a single `/health` — production-correct, signals to orchestrators that "process alive" and "ready to serve traffic" are different concerns
- Scalar over Swashbuckle 10 — Scalar is the modern recommendation and reads the OpenAPI document produced by the built-in `MapOpenApi()`, keeping us aligned with the .NET 10 way
- Kept FluentAssertions on `6.*` deliberately — version 7+ moved to a paid commercial license; staying on 6 keeps the option open to migrate to AwesomeAssertions or Shouldly if FA's commercial terms ever bite
- Skipped Testcontainers for now — the integration test only hits liveness, which has no dependencies. Real DB-touching tests in Week 2 will require either Testcontainers or a dedicated test DB; deferring that decision until we have a concrete repository to test
- Domain test landed in `Replyo.Application.Tests` rather than a separate `Replyo.Domain.Tests` project — overkill for one test, can split later if domain test count grows

---

## Day 3 — May 4, 2026

**Done:**

*Commit hygiene cleanup discovered along the way*
- Caught two pieces of PROGRESS-vs-reality drift from Day 2: the leftover `UnitTest1.cs` files in both `Replyo.Application.Tests` and `Replyo.Api.Tests` were never actually deleted (Day 2 PROGRESS claimed only one was), and the health endpoint integration test was never actually committed (was untracked despite Day 2 PROGRESS recording it as done)
- Recreated `HealthEndpointTests.cs` from the Day 2 chat history and committed it for real this time
- Removed both `UnitTest1.cs` files
- Test discovery now shows exactly two real tests with no scaffolding placeholders

*Auth plan locked and Day 2 `Next` rewritten*
- Hybrid Identity decided: pull `Microsoft.Extensions.Identity.Core` for `PasswordHasher<User>` only, skip Identity tables, EF stores, UI scaffolding, and `IdentityUser` base class — Domain layer stays clean, dependency leaks into Infrastructure only
- DB-tracked rotating refresh tokens with revoke support; HS256 JWT signing for single-API-service; self-serve tenant registration; `UserRole` enum added now to avoid future JWT claim-shape migrations; invite flow deferred to Week 2
- Rewrote Day 2's `Next (Day 4)` block to capture all locked decisions instead of the open questions it previously held — the `Next` block becomes the contract between sessions

*Auth packages added*
- `Microsoft.Extensions.Identity.Core` 10.0.2 in `Replyo.Infrastructure` for `PasswordHasher<User>` only
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.7 in `Replyo.Api` for JWT bearer middleware
- Initial attempt used the wrong package (`Microsoft.AspNetCore.Identity` 2.3.9 — the legacy ASP.NET-era package, no .NET Core / 10 release); corrected to `Microsoft.Extensions.Identity.Core` after restore failure

*Domain changes for auth*
- Added `UserRole` enum (`Owner = 1`, `Member = 2`) in `Replyo.Domain.Enums`
- Replaced `User.Create` with two role-specific factories — `User.CreateOwner` and `User.CreateMember` — both delegating to a private shared `Create`. Mirrors the `Message.FromVisitor / FromAssistant / FromHumanAgent` pattern for role-distinct entity construction
- Added `Role` property to `User` with private setter, set only via factories
- Added `RefreshToken` entity in `Replyo.Domain.Entities`: rotation chain support via `ReplacedByTokenHash`, `IsActive` / `IsExpired` / `IsRevoked` computed properties, audit IPs (`CreatedByIp` / `RevokedByIp`), `Issue` and `Revoke` lifecycle methods rather than generic `Create` / `Deactivate`

*Persistence configuration*
- `Role` mapped via `HasConversion<int>()` matching the explicit-int-value convention from Day 2
- `RefreshTokenConfiguration` with snake_case columns, unique index on `token_hash` (lookup), non-unique index on `user_id` (bulk revoke), cascade delete FK to `users(id)`
- Deliberately did NOT add a `User.RefreshTokens` navigation collection — refresh tokens are queried by hash, not via User. Symmetry-with-`Tenant.Users` was considered and rejected because the access patterns differ
- `EF Ignore` on the three computed properties (`IsActive` / `IsExpired` / `IsRevoked`)
- `RefreshTokens` `DbSet` added to `ReplyoDbContext`; configurations picked up automatically via `ApplyConfigurationsFromAssembly`

*Migration*
- `AddRefreshTokensAndUserRole` migration generated and applied
- Reviewed before applying: confirmed `role` column added as `integer NOT NULL`, `refresh_tokens` table with all 10 columns, both indexes, cascade FK
- EF added `DEFAULT 0` on the `role` column to allow migration on a populated table; harmless for us (no existing users), and `0` is not a defined enum value but factories always set Role explicitly so it can never appear in practice
- All 8 entity tables now present in Postgres plus `__EFMigrationsHistory`

*Commits*
- 7 commits today, all conventional and focused: `docs(progress)`, `chore(tests)` (×2), `chore(deps)`, `feat(domain)`, `feat(persistence)` (×2)


**Stuck / resolved:**
- `dotnet add package Microsoft.AspNetCore.Identity --version 10.0.7` failed with `NU1102: Unable to find package` — that package's last release is 2.3.9 from the framework era and has no modern equivalent. Resolved by switching to `Microsoft.Extensions.Identity.Core 10.0.2`, which is the correct package for `PasswordHasher<TUser>` in modern .NET. Cleaned up the bad reference with `dotnet remove package` before re-adding the right one
- Day 2 PROGRESS drift discovered: claimed deletions and a committed test that didn't match disk state. Root cause was likely conflating chat output with disk state — code generated in chat was treated as code on disk. Discipline going forward: PROGRESS bullets must reflect `git status` and `git log`, not chat history. Verify before writing


**Decisions:**
- Hybrid Identity over full `AddIdentity()` or rolled-from-scratch — gets Microsoft's PBKDF2 hasher with version-tagged forward-compatibility, avoids dragging `IdentityUser` into Domain, keeps the `User` entity clean
- DB-tracked rotating refresh tokens over stateless JWT refresh — supports per-device sessions, "log out everywhere," and refresh token theft detection via rotation chain replay detection
- HS256 over RS256 — single API service, asymmetric keys would be ceremony with no benefit
- `RefreshToken` is its own entity but not exposed as a `User` navigation collection — access patterns are by-token-hash; loading a user with all their refresh tokens would be actively bad for a high-churn table
- `Issue` / `Revoke` method names on `RefreshToken` instead of generic `Create` / `Deactivate` — token lifecycles have specific vocabulary and using it makes the domain self-documenting
- IP audit fields kept (`CreatedByIp` / `RevokedByIp`) — cheap to add now, costs a migration to add later, useful for "log out from suspicious IPs"
- `UserRole` enum added now even though invite flow is Week 2 — JWT will carry `role` claim from day one to avoid claim-shape migrations later
- Three commits for the domain/persistence/migration work even though they're tightly related — each commit reads as a coherent unit on its own. Future-self reading `git log` gets a clear story instead of one mega-commit

## Day 4 — May 5, 2026

**Done:**

*GitHub & CI (earlier in the day)*
- Set up GitHub remote, pushed initial commits
- CI workflow for backend with paths-ignore configuration
- Branch protection on main (verified by attempted direct push that was correctly blocked)
- Adjusted paths-ignore to drop `**.md` so docs PRs still run CI

*Application-layer auth foundation (commits 4a → 4c)*

Commit 4a — auth abstractions:
- `ICommandHandler<TCommand, TResult>` as the single application-layer handler contract; handlers self-validate via injected FluentValidation validators so the contract holds across HTTP, Hangfire, and SignalR entry points
- `IPasswordHasher` with three-state `PasswordVerificationOutcome` (`Failed` / `Success` / `SuccessRehashNeeded`) — supports transparent rehashing when stored PBKDF2 iteration counts age out
- `IJwtTokenService` returning an `IssuedTokens` record carrying both the plaintext refresh token (for the client) and its hash (for storage), keeping the hashing algorithm an implementation detail
- `JwtOptions` with data-annotation validation for fail-fast startup if the signing key is missing or too short for HS256

Commit 4b-prep-deps — pinned `FluentValidation` and `FluentValidation.DependencyInjectionExtensions` from `11.*` to `11.12.0`, matching the EF Core pinning from Day 2

Commit 4b-prep — extracted `AddApplication()` and `AddInfrastructure()` DI extension methods so Program.cs delegates layer registration. `IApplicationDbContext` interface added in Application layer; `ReplyoDbContext` implements it. `NoTenantCurrentTenant` placeholder moved from Program.cs to Infrastructure as `internal sealed`

Commit 4b — `RegisterTenant` command, handler, validator. Creates tenant + Owner user atomically in a single `SaveChangesAsync` transaction. Email uniqueness checked against the lowercase-normalized form to match `User.Create`'s internal normalization. Slug uniqueness via `SlugGenerator` + 6-char random suffix on collision. `ConflictException` added in `Common/Exceptions/`. `SlugGenerator` strips diacritics (Café → cafe) and falls back to "tenant" for emoji-only inputs

Commit 4b-followup (×2) — `Tenant.Create` enforces slug regex format invariant (`^[a-z0-9]+(-[a-z0-9]+)*$`); dropped `.ToLowerInvariant()` so the entity tells callers the rules instead of silently fixing bad input. Landed as two commits with the same subject because the `.ToLowerInvariant()` removal got missed in the first pass and was caught and committed separately

Commit 4c — `Login` and `RefreshToken` commands. LoginHandler returns generic "invalid email or password" on lookup or verification failure to prevent account enumeration; active-status check happens after password verification so deactivated-account messages only reach users who proved they own the account; transparent password rehashing fires on a successful login when stored hash uses outdated parameters. RefreshTokenHandler implements rotation chain replay detection: on presentation of an already-revoked token, all of the user's active refresh tokens are revoked (log out everywhere) since we can't distinguish legitimate replay from malicious. `UnauthorizedException` added. Folder named `RefreshTokens/` (plural) to avoid namespace collision with `Domain.Entities.RefreshToken`. All three handlers registered as scoped services in `AddApplication()`

*Test infrastructure (commits 4c-tests-prep + 4c-tests)*

- Pinned `FluentAssertions` (6.*) and `Moq` (4.*) to resolved versions; same convention as deps pinning above
- Removed unused `Microsoft.EntityFrameworkCore.Sqlite` package after the SQLite path failed
- Removed unused `Moq` package — no tests in the project use it; hand-rolled fakes (`FakePasswordHasher`, `FakeJwtTokenService`, `FakeCurrentTenant`) are explicit and debuggable. Mocking library decision deferred until a test genuinely needs one
- `[InternalsVisibleTo]` granted to `Replyo.Application.Tests` so internal handlers can be unit-tested without making them public
- `PostgresFixture` — xUnit `IClassFixture` running pgvector/pgvector:pg16 container per test class, real `MigrateAsync()` against the container, per-test data reset via `ResetDatabaseAsync()`
- `RefreshTokenHandlerTests` — control test (`HandleAsync_WhenPresentedWithValidToken_RotatesAndIssuesNewPair`) plus security test (`HandleAsync_WhenPresentedWithRevokedToken_RevokesAllActiveTokensForUser`) verifying replay detection revokes all active tokens for the user, not just the chain
- First test run pulled the pgvector image (~25s), subsequent runs are 8s end-to-end


**Stuck / resolved:**

- Initial sketch of `RegisterTenantHandler` had `IApplicationDbContext _db` typed as the concrete `ReplyoDbContext` — broke the dependency direction (Application can't reference Infrastructure). Fixed by introducing the `IApplicationDbContext` interface in Application and forwarding the DI registration in Infrastructure: `services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ReplyoDbContext>())`. Side effect: forced the DI extension method refactor

- `Microsoft.Extensions.Hosting.IHostEnvironment` not resolvable from Infrastructure project — Infrastructure has no business referencing the ASP.NET Core framework. Resolved by changing `AddInfrastructure(..., IHostEnvironment)` to `AddInfrastructure(..., bool isDevelopment)`. The Api layer translates `builder.Environment.IsDevelopment()` to a bool at the call site

- Day 2/Day 3 PROGRESS-vs-reality drift recurred this session in three places:
  - `FluentValidation` versions floating at `11.*` despite the Day 2 pinning convention — caught when reviewing the csproj for the test work
  - `Microsoft.EntityFrameworkCore.Sqlite` left in the test project after the SQLite path was abandoned — caught when listing packages
  - `[InternalsVisibleTo]` was assumed-added but I never verified — claimed it as "done" without reading disk, caught later when reviewing the csproj contents
  - Each was caught by reading actual disk state before claiming. Discipline going forward: `git diff HEAD` and `cat <file>` before asserting that something was done

- Namespace collision: creating the `Replyo.Application.Auth.Commands.RefreshToken` folder shadowed `Replyo.Domain.Entities.RefreshToken` everywhere in the Application project. Existing handlers compiled with `Domain.Entities.RefreshToken.Issue(...)` qualifications I'd added without thinking through the cause. Resolved by renaming the folder to `RefreshTokens/` (plural). Lesson: feature folders matching domain entity names need plural-or-different naming

- SQLite in-memory provider couldn't map the `vector(1536)` column on `KnowledgeChunk` — the EF model fails validation at first use even when no test queries that entity. Initially recommended SQLite confidently, then had to pivot to Testcontainers + real Postgres. The Testcontainers decision was deferred from Day 2 ("when we have a concrete repository to test"); the trigger fired with a real use case, which is the right time

- Slug invariant fix landed as two consecutive commits with the same subject because the `.ToLowerInvariant()` removal got missed in the first pass. Caught when reviewing `git log`. End state correct, log readability degraded. Fix: `git diff HEAD` before each commit to verify the diff matches the mental model

- First control test failed with a `duplicate key value violates unique constraint "IX_refresh_tokens_token_hash"` error. Cause: the seed used suffix `-1` for the seeded refresh token plaintext, and `FakeJwtTokenService` started its issue counter at 0 → the handler's first `Issue` call produced suffix `-1`, colliding with the seed. Fixed by starting the fake's counter at 100, leaving low suffixes for seed code. Real win: the failure surfaced because the test went through real Postgres unique constraint enforcement — SQLite or the in-memory provider would have silently accepted the duplicate and the test would have passed for the wrong reason

- Operating instructions claim GitHub commits are "intentionally deferred" — `origin/main` is real and we've been pushing. Update operating instructions on next major edit


**Decisions:**

- **Application-layer command/handler structure**: plain handlers with self-validation via `await _validator.ValidateAndThrowAsync(command, ct)` as the first line. `ICommandHandler<TCommand, TResult>` interface plus a marker interface per handler (`IRegisterTenantHandler : ICommandHandler<RegisterTenantCommand, RegisterTenantResult>`). No mediator, no endpoint filters for validation — handler self-validation works across HTTP, Hangfire, and SignalR entry points uniformly

- **File and folder layout**: file-per-type, folder-per-command. `Auth/Commands/RegisterTenant/{Command,Result,Validator,Handler}.cs`. Cross-cutting abstractions in `Common/Abstractions/`, options classes in `Common/Configuration/`

- **`JwtOptions` lives in Application**, not Infrastructure. Options describe a product behavior (token lifetime, issuer, audience), not an implementation detail. The `SigningKey` field is the one outlier — defensible because splitting the options class is more ceremony than it earns today

- **`IApplicationDbContext` abstraction over repositories.** Repositories over EF Core are an antipattern — `DbSet<T>` already is a repository, `IQueryable<T>` already is a query interface. The interface satisfies the dependency-direction requirement without forcing handlers to give up `Include`, projections, and query composition

- **Marker interface per handler over open-generic registration.** `services.AddScoped<IRegisterTenantHandler, RegisterTenantHandler>()` is greppable and reads as English at call sites. `ICommandHandler<RegisterTenantCommand, RegisterTenantResult>` registration is type-theoretic noise

- **Three-state `PasswordVerificationOutcome`** (`Failed` / `Success` / `SuccessRehashNeeded`) instead of bool. Models PBKDF2 iteration-count aging from day one so the LoginHandler can transparently rehash on outdated parameters

- **`bool` as the result type for genuinely-void commands** (when we eventually hit one). No `Unit` type, no parameterless `ICommandHandler<TCommand>`. Pre-decided so the choice doesn't get relitigated later

- **`Tenant.Create` enforces slug format via regex** (`^[a-z0-9]+(-[a-z0-9]+)*$`). Entity is the guardian of its own invariants; the production call path through `SlugGenerator` already produces conformant slugs, so no behavior change. Protects future callers (seed scripts, admin tooling) from accidentally creating tenants with broken slugs

- **Replay detection: revoke all active tokens for the user**, not the rotation chain. We can't distinguish legitimate replay from malicious, so the safer default is log-out-everywhere. Simpler than walking `ReplacedByTokenHash` chains and more defensive

- **Generic auth error messages** (`"Invalid email or password"`) prevent account enumeration. Active-status check happens after password verification so the "Account is deactivated" message only reaches users who proved they own the account

- **Hand-rolled test fakes over a mocking library.** Tonight's tests work cleanly without one; mocking libraries encourage a particular test style (call counts, argument matchers) that's often a code smell. Decision when we hit a test that genuinely needs one — likely NSubstitute for API ergonomics and clean maintainer history (Moq's 2023 SponsorLink incident makes it a non-starter for a portfolio project)

- **Testcontainers + Postgres for handler unit tests.** Real Postgres + pgvector image. Per-class `IClassFixture` (~5-10s startup, 1-2s for subsequent test classes), per-test data reset. SQLite alternative explored and rejected because of pgvector. Sets the pattern for all future Application-layer DB-touching tests

- **`InternalsVisibleTo` for tests** rather than making handlers public. Preserves the production encapsulation (callers depend on marker interfaces, not concrete classes) while letting tests construct handlers directly

- **Multiple commits over fewer larger commits** — five commits for what could have been one big "auth foundation" commit. Each reads as a coherent unit; future-self reading `git log` gets a clear story instead of one mega-commit


**Known issues (deferred, tracked for future commits):**

- TOCTOU race on email/slug uniqueness in `RegisterTenantHandler` — pre-check passes, unique constraint catches on save, surfaces as 500 instead of 409. Fix in commit 4d alongside the `IExceptionHandler` for `ConflictException` because the proper fix lives in API/Infrastructure where Npgsql can be referenced legitimately. Application layer mustn't depend on Npgsql

- SHA-256 hashing of refresh tokens duplicated across three sites: production `RefreshTokenHandler.HashRefreshToken`, test `FakeJwtTokenService.HashRefreshToken`, test `RefreshTokenHandlerTests.HashForTest`. Real `JwtTokenService` (commit 4d) will be the fourth. Extract to a shared helper in `Common/` when 4d lands

- **Tenant query filters not implemented in `ReplyoDbContext`** despite PROJECT_PLAN.md committing to row-level filtering. Currently no tenant-scoped queries exist (auth flow is cross-tenant by design), so the gap isn't actively bleeding. Fix in Week 2 commit 1, before any tenant-scoped query gets written. Auth handlers will need explicit `IgnoreQueryFilters()` calls

- Validator caps `OwnerFullName` and `TenantName` at 200 chars but doesn't cap the derived slug length. A 200-char tenant name produces a 200-char slug. Add `MaximumLength(100)` for slugs in a small follow-up

- `RefreshTokenHandler` and `FakeJwtTokenService` both use a counter to make refresh-token plaintexts deterministic. The fake starts at 100 to avoid collisions with low-suffix seed values (0, 1, 2, ...). Documented in the fake; future test authors using values >= 100 will collide and need to know


**Next (Day 5):**

Resume the auth plan at commit 4 of 6, but split into three smaller commits:

- **Commit 4d-i — Infrastructure layer:**
  - `JwtTokenService` implementation using `Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler` (the modern API; **not** `JwtSecurityTokenHandler` from the legacy stack)
  - `PasswordHasher` wrapping `Microsoft.Extensions.Identity.Core`'s `PasswordHasher<User>`, translating its `PasswordVerificationResult` to our three-state `PasswordVerificationOutcome`
  - SHA-256 refresh-token-hashing helper extracted to `Common/` (the fourth caller triggers extraction per the rule above)
  - DI registration in `AddInfrastructure`: bind `JwtOptions` from configuration with `ValidateDataAnnotations()` + `ValidateOnStart()`

- **Commit 4d-ii — Api layer:**
  - JWT bearer middleware (`AddAuthentication().AddJwtBearer(...)`)
  - `POST /api/auth/register | login | refresh` minimal-API endpoints (or controllers — decide before writing; minimal-APIs lean cleaner here)
  - Replace `NoTenantCurrentTenant` with real `HttpContextCurrentTenant` reading `tenant_id` from `ClaimsPrincipal`
  - `IExceptionHandler`s for `ConflictException` → 409, `UnauthorizedException` → 401, `ValidationException` → 400 with `ValidationProblemDetails`
  - TOCTOU race fix lands here: catch `DbUpdateException` with Postgres SQLSTATE 23505 in an Infrastructure-side `SaveChangesInterceptor` or inside the exception handler, translate to `ConflictException`
  - `RequireOwner` / `RequireMember` authorization policies registered (not yet attached to endpoints — Week 2 work)

- **Commit 4d-iii — Integration tests:**
  - `WebApplicationFactory<Program>`-based integration tests for register and login happy paths
  - Open question: share `PostgresFixture` between `Application.Tests` and `Api.Tests`, or have `Api.Tests` use the dev Docker Compose Postgres? Option 1 is more self-contained, Option 2 is faster. Decide before writing

Open question worth resolving before Day 5 starts:
- Operating instructions update — they're stale on the GitHub-commits-deferred point and silent on a few decisions made today (clean-architecture handler interface convention, Testcontainers for handler tests, hand-rolled fakes over mocking library, plural folder naming for entity-named features). Worth a small editing pass at the start of Day 5

---

**Next (Day 5):**

This session executed most of what the earlier Day 4 Next block called "commit 4 of 6": all three Application-layer auth commands (Register, Login, Refresh) plus their validators, plus test infrastructure (Testcontainers + replay-detection coverage) that the original plan had at the end. What remains is Infrastructure, Api, and integration tests — split into three smaller commits because the work is bigger than a single commit deserves.

*Workflow constraint:*
- All future commits go through PRs. Branch protection on `main` enforces this; direct push to main was verified blocked earlier on Day 4. The operating instructions still describe a "deferred GitHub commits" workflow that no longer applies — update on next major edit

*Resolved from the earlier Next block:*
- Application-layer command/handler structure question (mediator vs. plain services vs. endpoint filters) resolved in favor of plain handler classes with self-validation via injected `IValidator<T>`. `ICommandHandler<TCommand, TResult>` plus a marker interface per handler. No mediator. See Decisions section above for the reasoning

---

**Commit 4d-i — Infrastructure layer:**

- `JwtTokenService` implementing `IJwtTokenService`, using `Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler` (the modern API; **not** `JwtSecurityTokenHandler` from the legacy stack)
- `PasswordHasher` implementing `IPasswordHasher`, wrapping `Microsoft.Extensions.Identity.Core`'s `PasswordHasher<User>`. Translate Microsoft's `PasswordVerificationResult` (`Failed` / `Success` / `SuccessRehashNeeded`) to our three-state `PasswordVerificationOutcome`. The `User` generic parameter on Microsoft's hasher is required but doesn't actually constrain — the hash is opaque
- SHA-256 refresh-token-hashing helper extracted to `Common/` (the fourth caller — production handler, fake JWT service, test helper, and now the real JwtTokenService). The "extract on fourth caller" rule from this session's deferred-issues list fires here
- DI registration in `AddInfrastructure`:
  - `services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.SectionName).ValidateDataAnnotations().ValidateOnStart()` so startup fails loud if signing key is missing or too short
  - `services.AddSingleton<IJwtTokenService, JwtTokenService>()` (singleton — no per-request state)
  - `services.AddSingleton<IPasswordHasher, PasswordHasher>()` (singleton — same)
- No production code in this commit; all wiring. Build green and existing handler tests still pass against the real `IJwtTokenService` (replacing `FakeJwtTokenService` in a future test commit, not this one)

---

**Commit 4d-ii — Api layer:**

- JWT bearer middleware: `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => ...)` configured from the same `JwtOptions` instance Infrastructure binds. `TokenValidationParameters` set with `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey` all true
- Three minimal-API endpoints: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`. Each binds the corresponding command from the request body (with `[FromBody]`), captures `HttpContext.Connection.RemoteIpAddress` for the `CreatedByIp` field, and dispatches to the handler
  - Endpoints are `AllowAnonymous` because they're the path *to* authentication, not from it
  - Decision to make before writing: minimal APIs vs controllers. Lean is minimal APIs — the endpoints are thin glue, no need for the controller machinery. Worth one paragraph in the commit message either way
- Replace `NoTenantCurrentTenant` with real `HttpContextCurrentTenant`:
  - Reads `tenant_id` claim from `ClaimsPrincipal` if authenticated, returns null otherwise
  - Lives in Api layer (not Infrastructure) because it depends on `HttpContext`
  - DI registration moves from `AddInfrastructure` to `AddApi` (a new extension method, follows the same pattern as the other two)
- Three `IExceptionHandler` implementations registered in order:
  - `ValidationExceptionHandler` → 400 with `ValidationProblemDetails` (catches `FluentValidation.ValidationException`)
  - `ConflictExceptionHandler` → 409 with `ProblemDetails`
  - `UnauthorizedExceptionHandler` → 401 with `ProblemDetails`
  - Order matters in registration: more specific first, generic fallback last
- TOCTOU race fix lands here as part of the exception handling pipeline:
  - Catch `DbUpdateException` with Postgres SQLSTATE 23505 (unique violation), translate to `ConflictException`
  - Best place: a `SaveChangesInterceptor` registered on the DbContext in Infrastructure, *or* an exception handler that unwraps the `DbUpdateException`. Decide before writing. Interceptor is more reusable across handlers; exception handler is more local. Lean is interceptor
- `RequireOwner` and `RequireMember` authorization policies registered in `AddApi` but **not yet attached to endpoints** — that's Week 2 work when there are non-auth endpoints to protect

---

**Commit 4d-iii — Integration tests:**

- `WebApplicationFactory<Program>`-based integration tests in `Replyo.Api.Tests`:
  - `RegisterEndpointTests.PostRegister_WithValidPayload_Returns201AndTokenPair`
  - `LoginEndpointTests.PostLogin_WithValidCredentials_Returns200AndTokenPair`
  - `LoginEndpointTests.PostLogin_WithUnknownEmail_Returns401WithGenericMessage` (account-enumeration defense — verifies the API doesn't leak whether email exists)
  - `RefreshEndpointTests.PostRefresh_WithValidToken_Returns200AndNewPair`
  - That's four happy-path-ish tests. Negative cases beyond the enumeration test get added as needed in Week 2
- Open question worth resolving before writing:
  - Share `PostgresFixture` between `Application.Tests` and `Api.Tests`, or have `Api.Tests` use the existing dev Docker Compose Postgres directly?
    - Option 1 (shared fixture): self-contained, each test class manages its own container, ~5-10s startup per test class. Cleaner CI story (no separate service container needed)
    - Option 2 (dev Postgres): faster (no per-test container startup), but tests now depend on `docker-compose up -d` having been run, and CI needs a Postgres service container in the workflow yaml
  - Lean is Option 1 (shared `PostgresFixture` extracted to a shared test utility project, or duplicated as `ApiPostgresFixture` in `Api.Tests`). Trade-off: slower test runs, simpler dependencies. Worth one explicit decision when 4d-iii starts

---

*Open questions to resolve before Day 5 starts:*

- **Operating instructions need a small editing pass:**
  - GitHub-commits-deferred wording is stale (we have a remote and have been pushing all session)
  - Operating instructions are silent on the workflow change to PR-only
  - Could capture the architectural decisions made today (clean-architecture handler interface convention, plural folder naming for entity-named features, Testcontainers as the testing pattern, hand-rolled fakes over a mocking library)
  - Doesn't have to happen at the start of Day 5; can drift to whenever a major edit is happening anyway

- **Decide on minimal APIs vs controllers for the auth endpoints** before writing 4d-ii. Defer to start of 4d-ii rather than pre-deciding now — the decision is small enough that fresh context helps

- **Decide on `SaveChangesInterceptor` vs exception handler** for the unique-violation-to-ConflictException translation in 4d-ii. Same — defer to start of that commit