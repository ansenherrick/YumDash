# Deployment Notes

## Current State

The repo is cleaned up for GitHub publishing:

- duplicate top-level font assets removed
- local caches and build artifacts ignored
- hardcoded admin seed credentials removed
- runtime configuration switched to PostgreSQL
- ASP.NET data protection keys configured to persist in the database

## Hosting Recommendation

This app is a server-rendered ASP.NET Core MVC application. It should be deployed to a host that supports long-running .NET web apps directly, such as:

- Azure App Service
- Render
- Fly.io
- Railway

Vercel is not the recommended target for this codebase in its current architecture.

## Supabase Status

The app code is now pointed at PostgreSQL and is structured for Supabase:

1. `Npgsql.EntityFrameworkCore.PostgreSQL` is installed.
2. `Program.cs` uses PostgreSQL and database-backed data protection keys.
3. Production configuration expects `ConnectionStrings__DefaultConnection`.
4. Admin bootstrap stays environment-driven through `SeedAdmin__Email` and `SeedAdmin__Password`.

## Current Blocker

The remaining missing piece is the checked-in EF migration set for PostgreSQL. This environment still does not have `dotnet-ef` available, so the migration files were not generated here.

Run this before the first real Supabase deployment:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialPostgresSchema --project src/YumDash.Web/YumDash.Web.csproj --startup-project src/YumDash.Web/YumDash.Web.csproj --output-dir Data/Migrations
```

## Suggested Production Sequence

1. Push the cleaned repo to GitHub.
2. Generate and commit the PostgreSQL migration files.
3. Create the Supabase project and database credentials.
4. Create the Azure App Service web app.
5. Set `ConnectionStrings__DefaultConnection` in Azure to the Supabase connection string.
6. Set `SeedAdmin__Email` and `SeedAdmin__Password` for the first deployment only.
7. Deploy and verify the app.
