# Ctrl+Alt+DeLorean

A scheduling REST API built with ASP.NET Core Minimal API. Users can create events, invite participants, and — once conflict detection is implemented — automatically identify and resolve scheduling overlaps.

## Tech Stack

- **Runtime:** .NET 10
- **Framework:** ASP.NET Core Minimal API
- **Database:** SQLite (`scheduling.db`)
- **ORM:** Entity Framework Core 10
- **Docs:** OpenAPI (built-in ASP.NET Core)

## Getting Started

```bash
dotnet run
```

The database is created automatically on first run via `EnsureCreated()`. The OpenAPI spec is available at `/openapi/v1.json`.

## API Endpoints

### Users

| Method | Path           | Description        |
|--------|----------------|--------------------|
| GET    | /users         | List all users     |
| GET    | /users/{id}    | Get a user by ID   |
| POST   | /users         | Create a user      |
| PUT    | /users/{id}    | Update a user      |
| DELETE | /users/{id}    | Delete a user      |

**Create/Update request body:**
```json
{ "name": "Jane Doe", "email": "jane@example.com" }
```

### Events

| Method | Path           | Description        |
|--------|----------------|--------------------|
| GET    | /events        | List all events    |
| GET    | /events/{id}   | Get an event by ID |
| POST   | /events        | Create an event    |
| PUT    | /events/{id}   | Update an event    |
| DELETE | /events/{id}   | Delete an event    |

**Create/Update request body:**
```json
{
  "title": "Team Sync",
  "startTime": "2026-05-20T09:00:00Z",
  "endTime": "2026-05-20T10:00:00Z",
  "organizerId": 1
}
```

Times are stored in UTC. Any timezone offset in the request is normalized on write.

### Participants

| Method | Path                                        | Description                  |
|--------|---------------------------------------------|------------------------------|
| GET    | /events/{eventId}/participants              | List participants for an event |
| POST   | /events/{eventId}/participants/{userId}     | Add a participant            |
| DELETE | /events/{eventId}/participants/{userId}     | Remove a participant         |

## Data Model

```
User
  Id, Name, Email
  → organizes many Events
  → participates in many Events (via EventParticipant)

Event
  Id, Title, StartTime (UTC), EndTime (UTC), OrganizerId
  → has many Participants (via EventParticipant)

EventParticipant
  EventId + UserId (composite PK)
```

## Conflict Detection

Not yet implemented. Planned:

- `GET /events/conflicts` — list overlapping events for a user
- Alternative time slot suggestions

Conflict rule: two events overlap when `StartA < EndB AND StartB < EndA`.

## Stretch Goals

- Conflict detection and alternative slot suggestions
- Recurring events
- Email notifications
- Calendar UI (Blazor or React)
