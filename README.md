# gci409 System Design Studio

`gci409` is an enterprise-focused platform for transforming project requirements and constraints into structured design outputs such as UML diagrams, architecture summaries, ERDs, DFDs, API suggestions, database guidance, and related technical artifacts.

This repository contains:

- an ASP.NET Core Web API backend
- a background worker for asynchronous artifact generation
- a React + TypeScript frontend
- PostgreSQL-backed persistence

## Backend Stack

- .NET 8
- ASP.NET Core Web API
- EF Core with PostgreSQL
- FluentValidation
- Serilog
- Swagger / OpenAPI
- JWT authentication with refresh tokens
- Docker and Docker Compose

## Solution Structure

```text
src/backend/
├─ Gci409.Api              # HTTP API host, middleware, Swagger, auth wiring
├─ Gci409.Application      # Use-case services, DTOs, validators, contracts
├─ Gci409.Domain           # Domain entities, aggregates, enums, invariants
├─ Gci409.Infrastructure   # EF Core, security, audit, generation engines
└─ Gci409.Worker           # Background processing host for generation jobs

tests/
├─ Gci409.ApplicationTests
└─ Gci409.ArchitectureTests
```

## Implemented Backend Capabilities

- authentication and refresh token flow
- user profile retrieval and update
- project creation, update, archive, and collaborator management
- requirement and constraint capture with versioning
- recommendation generation
- asynchronous generation request queueing and worker execution
- artifact storage and artifact version history
- UML-aware artifact generation outputs in PlantUML and Mermaid
- export creation, listing, retrieval, and text download
- project comment threads and collaboration notes
- template management and versioning
- project and platform audit log access
- platform admin user management and role visibility
- health check endpoint at `/health`

## Key API Areas

- `/api/auth`
- `/api/me`
- `/api/projects`
- `/api/projects/{projectId}/collaborators`
- `/api/projects/{projectId}/requirements`
- `/api/projects/{projectId}/recommendations`
- `/api/projects/{projectId}/generation-requests`
- `/api/projects/{projectId}/artifacts`
- `/api/projects/{projectId}/exports`
- `/api/projects/{projectId}/templates`
- `/api/projects/{projectId}/comments/threads`
- `/api/projects/{projectId}/audit-logs`
- `/api/admin`

The target REST contract is documented in `docs/api/rest-api-design.md`.

## Local Development

### Prerequisites

- .NET SDK 8
- Node.js 20+
- Docker Desktop
- PostgreSQL 16+ if not using Docker Compose

### Run with Docker Compose

```bash
docker compose up --build
```

Services:

- API: `http://localhost:5099`
- Swagger UI: `http://localhost:5099/swagger`
- Frontend: `http://localhost:5173`
- PostgreSQL: `localhost:5432`

### Run Backend Locally

```bash
dotnet run --project src/backend/Gci409.Api/Gci409.Api.csproj
dotnet run --project src/backend/Gci409.Worker/Gci409.Worker.csproj
```

## Configuration

Primary backend configuration lives in:

- `src/backend/Gci409.Api/appsettings.json`
- `src/backend/Gci409.Worker/appsettings.json`

Important settings:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:SigningKey`
- `Jwt:ExpiryMinutes`

## Notes on Security

- access tokens are JWT bearer tokens
- refresh tokens are stored hashed before persistence
- the first registered user is automatically bootstrapped as `PlatformAdmin`
- project-level authorization is enforced in application services
- platform admin endpoints require the `PlatformAdmin` role

## Testing

```bash
dotnet test tests/Gci409.ApplicationTests/Gci409.ApplicationTests.csproj
dotnet test tests/Gci409.ArchitectureTests/Gci409.ArchitectureTests.csproj
```

## Database and Schema Notes

PostgreSQL schema guidance is documented in `docs/database/postgresql-schema.md`.

The current runtime uses EF Core model creation on startup. A migration-based deployment flow should be introduced before production rollout.

## Engineering Direction

The backend follows a clean-architecture modular monolith approach:

- domain logic stays in `Gci409.Domain`
- orchestration and validation stay in `Gci409.Application`
- infrastructure concerns stay in `Gci409.Infrastructure`
- API and worker hosts remain thin

Artifact generation and recommendation logic are exposed behind interfaces so the current rule-based implementation can evolve toward richer rule packs or model-assisted generation without rewriting the rest of the system.
