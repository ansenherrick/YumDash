# Architecture Overview

## Goals

Build a restaurant web app that demonstrates public customer workflows, admin operations, reporting, and basic analytics.

## Application Layers

### Presentation

- ASP.NET Core MVC for public and admin pages
- Razor views for forms and management screens
- React for dynamic menu filtering

### Application/Data

- Entity Framework Core with SQL Server
- ASP.NET Core Identity for admin authentication
- LINQ-based dashboard queries

### Analytics

- Python script reads exported reservation data
- Generates weekly summaries and busiest-day insights

## Domain Assumptions

- Reservations include party size and a per-guest estimate for revenue calculations
- Menu item allergens are stored as a comma-separated list for portfolio simplicity
- Messages and event inquiries are stored for admin review and future feature expansion

## Suggested Next Enhancements

- Add pagination and server-side filtering to the admin menu grid
- Add email notifications for reservation status updates
- Add chart visualizations to the dashboard
- Add automated tests for controllers and services
