# Ctrl+Alt+DeLorean

## Overview
Ctrl+Alt+DeLorean is a scheduling system that allows users to create and manage events while automatically detecting and resolving scheduling conflicts.

## Features
- Create, update, delete events
- Invite participants
- Conflict detection (overlapping events)
- Suggest alternative time slots
- Time zone support

## Tech Stack
- ASP.NET Core MVC or Web API
- Entity Framework Core
- SQL Server
- NodaTime for time zone handling

## Architecture
- Clean architecture approach
- Services:
  - EventService
  - ConflictDetectionService
  - SchedulingService

## Key Concepts
- Time overlap detection algorithms
- Time zone normalization (UTC storage)
- Business rule enforcement

## Example Endpoints
- POST /api/events
- GET /api/events/user/{id}
- POST /api/events/{id}/invite
- GET /api/events/conflicts

## Conflict Logic
- Two events conflict if:
  - StartA < EndB AND StartB < EndA

## Stretch Goals
- Add calendar UI (Blazor or React frontend)
- Email notifications
- Recurring events