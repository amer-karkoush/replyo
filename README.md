# Replyo

AI-powered customer support platform for small businesses.

Replyo lets any business train an AI on their docs and website, embed a chat widget on their site, and handle customer questions automatically — with a dashboard for live conversations, analytics, and manual takeover when the AI hands off.

## Status

Active development. See `PROGRESS.md` for the latest.

## Stack

**Backend:** ASP.NET Core 10, EF Core 10, PostgreSQL + pgvector, Hangfire, SignalR, OpenAI API
**Frontend:** React 18, TypeScript, Vite, TanStack Query, Tailwind, shadcn/ui
**Widget:** Vanilla TypeScript bundle, iframe-isolated for cross-site embedding
**Infrastructure:** Docker Compose for local dev, GitHub Actions for CI/CD

## Architecture

Multi-tenant SaaS with three deployable artifacts:

1. **Backend API** — ASP.NET Core, handles auth, tenants, knowledge ingestion, AI orchestration, real-time chat
2. **Dashboard frontend** — React app where business owners configure their AI and monitor conversations
3. **Embeddable widget** — small JavaScript bundle businesses paste into their site for customer-facing chat

See `docs/architecture.md` for system design and key decisions.

## Getting started

(Local setup instructions will be added once scaffolding is complete.)

## License

MIT