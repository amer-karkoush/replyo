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
- Made 5 well-structured commits with conventional commit messages

**Stuck / resolved:**
- Windows Defender blocked initial dotnet run; resolved by adding C:\personal-projects to exclusions
- Tailwind v4 has removed `init` command and breaks shadcn/ui workflow; downgraded to v3 deliberately

**Decisions:**
- Targeting .NET 10 (current LTS) instead of .NET 8 — fully compatible with chosen libraries
- Tailwind CSS v3 instead of v4 — better shadcn/ui ecosystem support
- Working entirely locally for now; GitHub remote will come later

**Next (Day 2):**
- Install NuGet packages (EF Core, Npgsql, Serilog, FluentValidation, Hangfire)
- Configure connection string and Postgres connection from .NET
- Define Domain entities (User, Tenant, KnowledgeDocument, etc.)
- Set up EF Core DbContext
- Run first migration
- Replace WeatherForecast with a real /health endpoint