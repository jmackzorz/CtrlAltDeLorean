# Project: Ctrl+Alt+DeLorean

## Overview
A scheduling REST API that lets users create and manage events, invite participants, and detect scheduling conflicts. Core feature (conflict detection) is not yet implemented.

## Stack
- Language:       C# 13
- Runtime:        .NET 10
- Framework:      ASP.NET Core Minimal API
- Database:       SQLite (`scheduling.db`)
- ORM/Query:      Entity Framework Core 10 (`EnsureCreated`, no migrations)
- Auth:           None
- Testing:        None
- Package mgr:    NuGet / dotnet CLI
- CI/CD:          None

## Project Structure
```
Models/       — domain entities: User, Event, EventParticipant
Data/         — AppDbContext and EF model configuration
Program.cs    — all route handlers, DTOs, and request records (single file)
appsettings.json — SQLite connection string ("Default": "Data Source=scheduling.db")
```

## Architecture Decisions
- **Minimal API, not MVC controllers** — all routes registered via `MapGroup` in `Program.cs`. Do not introduce controllers.
- **No service or repository layer yet** — handlers are thin and call EF Core directly. Introduce a service layer only when conflict detection logic warrants it.
- **DTOs and request records are defined at the bottom of `Program.cs`** as C# `record` types. Keep them there until the file warrants splitting.
- **UTC enforcement at two levels**: `ToUniversalTime()` called in route handlers AND a `ValueConverter` in `OnModelCreating`. Both are intentional — don't remove either.
- **SQLite for simplicity** — README mentions SQL Server as a goal but SQLite is the actual implementation. Do not switch providers without discussion.
- **`EnsureCreated()` instead of migrations** — schema changes are destructive until migrations are added. Flag any model changes that would break the existing DB.
- **`EventParticipant` uses a composite primary key** `(EventId, UserId)` — duplication is rejected at the DB level and checked in the handler.
- **Implicit usings and file-scoped namespaces** are enabled project-wide.

## Coding Conventions
- Naming:         PascalCase for types and properties; camelCase for locals and route group vars
- Record types:   Use `record` for all DTOs and request bodies
- Collections:    Initialize with `[]` (C# 12 collection expressions), not `new List<>()`
- Strings:        Initialize with `string.Empty`, not `""`
- Nav properties: Initialize non-nullable EF navigation properties with `null!`
- Imports:        Namespace per file, file-scoped (`namespace Foo;`)
- Comments:       Minimal — section dividers in `Program.cs` use the `// ── Label ───` style

## Testing Expectations
No tests exist yet. When added, integration tests against a real SQLite in-memory DB are preferred over mocked DbContext. Unit tests are appropriate for pure conflict detection logic when that service is built.

## Dependencies
```
Approved:
  Microsoft.AspNetCore.OpenApi 10.*     — OpenAPI docs (already in use)
  Microsoft.EntityFrameworkCore.Sqlite  — SQLite provider (already in use)
  Microsoft.EntityFrameworkCore.Design  — EF tooling, dev only (already in use)

Not yet added (stretch goals from README — discuss before introducing):
  NodaTime                              — time zone handling
  Any email/notification library
```

## Off-Limits Areas
- Do not add EF migrations without discussion — `EnsureCreated()` is the current approach and migrations require coordination.
- Do not switch from SQLite to SQL Server without explicit instruction.
- Do not introduce NodaTime or replace the `DateTime` + UTC pattern without discussion.
- Do not refactor `Program.cs` into separate files unless the scope of a task directly requires it.

## Current Focus
Core CRUD scaffolding is complete (Users, Events, Participants). The next major feature is conflict detection — identifying overlapping events for a user and suggesting alternative time slots. The conflict algorithm is: two events conflict when `StartA < EndB AND StartB < EndA`.

Planned endpoints not yet built:
- `GET /events/conflicts` — list conflicting events for a user
- Alternative time slot suggestions

## Known Issues / Tech Debt
- All route handlers and types live in `Program.cs` — acceptable now, will need splitting once conflict/scheduling logic is added.
- `EnsureCreated()` blocks schema evolution; needs migration support before any non-trivial model change.
- No authentication or authorization — all endpoints are open.
- README references `EventService`, `ConflictDetectionService`, and `SchedulingService` but none exist yet.
- README mentions NodaTime but it is not installed; `DateTime` + UTC conversion is the current approach.
