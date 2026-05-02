# Replyo — Project Plan

## What it is

A multi-tenant SaaS platform where small businesses train an AI on their content, embed a chat widget on their website, and manage customer conversations from a dashboard.

## Architecture overview

- **Multi-tenant by design** — each business is an isolated tenant with their own data, AI configuration, and embedded widget
- **Three deployable artifacts:** backend API (.NET), dashboard frontend (React), embeddable widget bundle
- **AI layer:** OpenAI for chat completions and embeddings, pgvector for semantic search over each tenant's knowledge base
- **Real-time chat** via SignalR for both customer-facing widget conversations and the owner dashboard

## Stack

### Backend
- ASP.NET Core 10 Web API (clean architecture: Domain, Application, Infrastructure, Api layers)
- Entity Framework Core 10 with PostgreSQL provider
- PostgreSQL 16 + pgvector extension
- Hangfire for background job processing
- FluentValidation for request validation
- Serilog for structured logging
- SignalR for real-time chat
- xUnit + FluentAssertions for testing

### Frontend (dashboard)
- React 18 + TypeScript
- Vite for build tooling
- TanStack Query for server state
- React Router for routing
- Tailwind CSS + shadcn/ui for UI components
- Recharts for analytics visualizations
- Vitest + React Testing Library for testing

### Widget
- Vanilla TypeScript, bundled as a single .js file
- Iframe-isolated for style safety on third-party sites
- Embeddable via single script tag

### AI
- OpenAI gpt-4o-mini (default), gpt-4o (complex tasks)
- OpenAI text-embedding-3-small for vectors
- pgvector for storage and similarity search

### Infrastructure
- Docker Compose for local Postgres + Redis
- GitHub Actions for CI/CD (planned)
- Railway for backend + DB hosting (planned)
- Vercel for frontend dashboard (planned)
- Sentry for error monitoring (planned)

## Scope

### In scope for v1
- Tenant signup and workspace
- Document upload and URL scraping into the knowledge base
- AI configuration per tenant (system prompt, voice, escalation rules)
- Embeddable widget that businesses paste on their site
- Real-time chat between visitor and AI via SignalR
- Owner dashboard with live conversations and analytics
- Manual takeover (owner can jump into a conversation)
- Escalation logic (AI knows when to hand off)
- Conversation history and search
- Multi-tenant data isolation

### Deliberately out of scope for v1
- Stripe billing (architecture supports it; not built)
- SOC2 / advanced compliance
- Voice/phone integration
- Native mobile apps
- Slack/Teams integrations
- Multi-language support beyond English

## Timeline

- **Week 1 — Foundation:** repo setup, .NET solution, React skeleton, Docker, auth, multi-tenant middleware, deployed shell
- **Week 2 — Knowledge ingestion + AI core:** document upload, URL scraping, chunking, embeddings into pgvector, RAG retrieval pipeline
- **Week 3 — Real-time chat + embeddable widget:** SignalR chat infrastructure, widget bundle with iframe isolation, live demo site
- **Week 4 — Intelligence + polish + launch:** analytics, manual takeover, escalation logic, README polish, demo video, LinkedIn posts

## Repository structure

replyo/
├── backend/          # ASP.NET Core solution
├── frontend/         # React dashboard
├── widget/           # Embeddable widget bundle
├── docs/             # Architecture, ADRs, API docs
├── scripts/          # Utility scripts
├── docker-compose.yml
├── .gitignore
├── README.md
├── PROJECT_PLAN.md
└── PROGRESS.md

## Key architectural decisions

- **pgvector inside Postgres instead of a dedicated vector DB** — keeps the stack tight, avoids operational overhead, sufficient for our scale
- **Iframe-isolated widget** — prevents host site CSS from conflicting with widget styles, isolates security context
- **Clean architecture in the .NET backend** — Domain, Application, Infrastructure, Api layers with controlled dependencies
- **Multi-tenant via row-level filtering** — single database, tenant_id column on all tenant-scoped tables, EF Core query filters enforce isolation