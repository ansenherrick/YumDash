# YumDash

YumDash is an ASP.NET Core MVC restaurant management app with a public menu, reservation intake, event inquiries, contact workflows, and an admin back office for day-to-day operations.

## Features

- Public menu with client-side category, price, and allergen filtering
- Reservation request form
- Private event inquiry form
- Contact form
- Admin login and role-based access control
- Admin CRUD for menu items
- Reservation status management
- Weekly reservation CSV export
- Dashboard with operational summary metrics

## Tech Stack

- `ASP.NET Core MVC`
- `Entity Framework Core`
- `ASP.NET Core Identity`
- `PostgreSQL`
- `Supabase` as the intended hosted PostgreSQL target
- `React` via ESM imports for menu filtering
- `Python` for offline analytics/reporting

## Project Structure

- `src/YumDash.Web` - main ASP.NET Core MVC application
- `analytics` - Python reporting scripts
- `docs` - architecture notes, deployment notes, and planning artifacts

## Local Setup

1. Install the `.NET 10 SDK`.
2. From the repo root, run:
   - `dotnet build YumDash.slnx`
   - `dotnet run --project src/YumDash.Web/YumDash.Web.csproj`
3. The development profile expects a PostgreSQL database and defaults to:
   - `Host=localhost;Port=5432;Database=yumdash_dev;Username=postgres;Password=postgres`
4. Override `ConnectionStrings__DefaultConnection` if your local PostgreSQL credentials differ.

## Admin Bootstrap

No default admin credentials are committed to the repo.

If you want startup to seed an admin user, provide these environment variables before running the app:

- `SeedAdmin__Email`
- `SeedAdmin__Password`

Example:

```bash
export SeedAdmin__Email="admin@example.com"
export SeedAdmin__Password="ChangeMe123"
dotnet run --project src/YumDash.Web/YumDash.Web.csproj
```

## GitHub Readiness

Local-only artifacts are ignored and should not be committed:

- build output (`bin/`, `obj/`)
- local SDK/cache folders (`.dotnet/`, `.nuget/`, `.lavish/`)
- macOS metadata files (`.DS_Store`)

## Deployment Notes

See [docs/deployment.md](docs/deployment.md) for the current Azure plus Supabase deployment path, remaining migration work, and production configuration notes.
