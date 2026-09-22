# Expense Tracker

A full-stack personal finance dashboard: track income and expenses, set monthly
budgets per category, import transactions from CSV, and see spending trends
on a dashboard with charts.

**Stack:** Angular 19 (standalone components, signals) · ASP.NET Core 10 Web API ·
PostgreSQL · EF Core · JWT auth · Chart.js

## Features

- Email/password auth with JWT, scoped so every user only sees their own data
- Transactions: create/edit/delete, filter by category/type/date, paginated
- CSV import (date, amount, type, category, note columns) with per-row error reporting
- Per-category monthly budgets with live spent-vs-limit progress
- Dashboard: month-to-date income/expense/balance, 6-month trend chart,
  spending-by-category breakdown, recent transactions
- Categories are seeded on signup and fully editable (name, color, icon, type)

## Project layout

```
server/ExpenseTracker.Api/   ASP.NET Core Web API (controllers, EF Core, JWT)
client/                      Angular app
docker-compose.yml           Postgres + API + client, for containerized runs
```

## Running locally (no Docker)

**Prerequisites:** .NET 10 SDK, Node 20+, PostgreSQL running locally.

### 1. Database

Create a dedicated role and database (only needs to be done once):

```bash
psql -U postgres -h localhost -f server/setup-db.sql
```

This creates a `expense_app` role and an `expense_tracker` database it owns.
(If your Postgres install requires a password for the `postgres` superuser,
you'll be prompted for it.)

### 2. API

```bash
cd server/ExpenseTracker.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=expense_tracker;Username=expense_app;Password=ExpenseApp_Dev_2026!"
dotnet user-secrets set "Jwt:Secret" "<any long random string>"
dotnet ef database update   # applies migrations
dotnet run
```

The API listens on `http://localhost:5254`. Swap the password in `setup-db.sql`
before using this anywhere but your own machine.

### 3. Client

```bash
cd client
npm install
npm start
```

Opens on `http://localhost:4200` and talks to the API via `src/environments/environment.ts`.

## Running with Docker

```bash
cp .env.example .env   # set DB_PASSWORD and JWT_SECRET
docker compose up --build
```

Client on `http://localhost:8080`, API on `http://localhost:5254`, Postgres on `5432`.
(Not tested in this environment — Docker wasn't available here — but the
Dockerfiles and compose file follow the standard multi-stage build pattern.)

## Design notes

- Categorical chart colors follow a validated, colorblind-safe palette (see
  `client/src/styles.scss` for the CSS custom properties, both light and dark).
- JWT secret and DB connection string are kept out of source control via
  `dotnet user-secrets` in dev, and environment variables in the Docker setup.
- The CSV importer creates any category it doesn't recognize rather than
  rejecting the row, so a full statement export can be imported in one pass.
