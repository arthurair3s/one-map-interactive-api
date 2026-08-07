# One Piece Interactive Map — API

ASP.NET Core Web API for the [One Piece Interactive Map](../docs/project_overview.md) project: persistence, business validation and the REST contract consumed by the frontend.

For the full data model, business rules (RN01–RN12) and API contract, see [`backend-planning.md`](../docs/backend-planning.md). For the internationalization plan, see [`i18n-planning.md`](../docs/i18n-planning.md).

## Stack

- ASP.NET Core 10 (single-project, namespace-separated "simplified Clean Architecture": `Domain` / `Application` / `Infrastructure` / `Api`)
- Entity Framework Core 10 + Npgsql (PostgreSQL 17)
- FluentValidation
- Scalar (OpenAPI UI)

## Prerequisites

- [Docker](https://www.docker.com/) and Docker Compose
- (Optional, for running outside Docker) [.NET 10 SDK](https://dotnet.microsoft.com/)

## Running with Docker (recommended)

Commands below run from the **repository root** (one level up from this folder), where `docker-compose.yml` and `.env.example` live.

```bash
cp .env.example .env
docker compose up -d --build
```

This starts:

- **postgres** — PostgreSQL 17, exposed on host port `5435` by default (`POSTGRES_PORT` in `.env`)
- **api** — the backend, exposed on host port `5000` by default (`API_PORT` in `.env`)

On startup (Development environment only), the API automatically applies pending EF Core migrations and re-seeds the database from `src/OnePieceMap.Infrastructure/Seed/seed-data.json` (the seed truncates and reloads every time, so IDs never drift between runs).

Once running:

- API base URL: `http://localhost:5000/api/v1`
- Scalar API reference (interactive docs): `http://localhost:5000/scalar/v1`

Stop everything with:

```bash
docker compose down
```

Add `-v` to also drop the Postgres volume (full reset).

## Running locally without Docker

Requires a PostgreSQL instance reachable via a connection string in `src/OnePieceMap.Api/appsettings.Development.json` (or the `ConnectionStrings__DefaultConnection` environment variable).
||||

```bash
cd src
dotnet restore
dotnet run --project OnePieceMap.Api.csproj
```

Migrations and seeding also run automatically on startup in Development.

## Project structure

```
one-map-interactive-api/
├── Dockerfile
├── OnePieceMap.slnx
└── src/
    ├── OnePieceMap.Api/              # Controllers, middleware, DI, OpenAPI/Scalar
    ├── OnePieceMap.Application/      # Services, DTOs, validators — organized by feature
    │   └── Features/
    │       ├── Sagas/  Arcs/  Islands/  ArcIslands/
    │       ├── Characters/  CharacterVersions/
    │       ├── Events/  EventParticipants/
    │       └── Wiki/                 # read-only endpoints tailored for the frontend map
    ├── OnePieceMap.Domain/           # Entities, enums — no framework dependencies
    └── OnePieceMap.Infrastructure/   # DbContext, EF configurations, migrations, seed data
```

## Useful commands

```bash
# Add a new EF Core migration
dotnet ef migrations add <MigrationName> --project src/OnePieceMap.Infrastructure --startup-project src/OnePieceMap.Api
```

## Environment variables

Set in `.env` at the repository root (see `.env.example`):

| Variable            | Default       | Description                  |
| ------------------- | ------------- | ---------------------------- |
| `POSTGRES_USER`     | `postgres`    | Postgres user                |
| `POSTGRES_PASSWORD` | `postgres`    | Postgres password            |
| `POSTGRES_DB`       | `onepiecemap` | Database name                |
| `POSTGRES_PORT`     | `5435`        | Host port mapped to Postgres |
| `API_PORT`          | `5000`        | Host port mapped to the API  |
