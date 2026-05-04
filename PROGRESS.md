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

**Next (Day 4):**
- Begin auth scaffolding: JWT bearer authentication, password hashing (BCrypt or ASP.NET Core Identity's PasswordHasher)
- Real `HttpContextCurrentTenant` implementation that reads tenant ID from JWT claims, replacing the temporary `NoTenantCurrentTenant`
- Auth endpoints: register, login, refresh
- Decide whether to use ASP.NET Core Identity wholesale or roll a leaner custom auth (Identity is heavier than we need for a multi-tenant API; worth a deliberate decision)