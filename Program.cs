using Microsoft.EntityFrameworkCore;
using CtrlAltDeLorean.Data;
using CtrlAltDeLorean.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

app.MapOpenApi();
app.UseHttpsRedirection();

// ── Users ──────────────────────────────────────────────────────────────────

var users = app.MapGroup("/users").WithTags("Users");

users.MapGet("/", async (AppDbContext db) =>
    await db.Users
        .Select(u => new UserDto(u.Id, u.Name, u.Email))
        .ToListAsync());

users.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    return user is null
        ? Results.NotFound()
        : Results.Ok(new UserDto(user.Id, user.Name, user.Email));
});

users.MapPost("/", async (CreateUserRequest req, AppDbContext db) =>
{
    var user = new User { Name = req.Name, Email = req.Email };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", new UserDto(user.Id, user.Name, user.Email));
});

users.MapPut("/{id:int}", async (int id, UpdateUserRequest req, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    user.Name = req.Name;
    user.Email = req.Email;
    await db.SaveChangesAsync();
    return Results.Ok(new UserDto(user.Id, user.Name, user.Email));
});

users.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ── Events ─────────────────────────────────────────────────────────────────

var events = app.MapGroup("/events").WithTags("Events");

events.MapGet("/", async (AppDbContext db) =>
    await db.Events
        .Select(e => new EventDto(e.Id, e.Title, e.StartTime, e.EndTime, e.OrganizerId))
        .ToListAsync());

events.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var ev = await db.Events.FindAsync(id);
    return ev is null
        ? Results.NotFound()
        : Results.Ok(new EventDto(ev.Id, ev.Title, ev.StartTime, ev.EndTime, ev.OrganizerId));
});

events.MapPost("/", async (CreateEventRequest req, AppDbContext db) =>
{
    if (!await db.Users.AnyAsync(u => u.Id == req.OrganizerId))
        return Results.BadRequest("Organizer not found.");

    var ev = new Event
    {
        Title = req.Title,
        StartTime = req.StartTime.ToUniversalTime(),
        EndTime = req.EndTime.ToUniversalTime(),
        OrganizerId = req.OrganizerId
    };
    db.Events.Add(ev);
    await db.SaveChangesAsync();
    return Results.Created($"/events/{ev.Id}", new EventDto(ev.Id, ev.Title, ev.StartTime, ev.EndTime, ev.OrganizerId));
});

events.MapPut("/{id:int}", async (int id, UpdateEventRequest req, AppDbContext db) =>
{
    var ev = await db.Events.FindAsync(id);
    if (ev is null) return Results.NotFound();

    if (!await db.Users.AnyAsync(u => u.Id == req.OrganizerId))
        return Results.BadRequest("Organizer not found.");

    ev.Title = req.Title;
    ev.StartTime = req.StartTime.ToUniversalTime();
    ev.EndTime = req.EndTime.ToUniversalTime();
    ev.OrganizerId = req.OrganizerId;
    await db.SaveChangesAsync();
    return Results.Ok(new EventDto(ev.Id, ev.Title, ev.StartTime, ev.EndTime, ev.OrganizerId));
});

events.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var ev = await db.Events.FindAsync(id);
    if (ev is null) return Results.NotFound();
    db.Events.Remove(ev);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ── Event Participants ─────────────────────────────────────────────────────

events.MapGet("/{eventId:int}/participants", async (int eventId, AppDbContext db) =>
{
    if (!await db.Events.AnyAsync(e => e.Id == eventId))
        return Results.NotFound();

    var participants = await db.EventParticipants
        .Where(ep => ep.EventId == eventId)
        .Select(ep => new ParticipantDto(ep.EventId, ep.UserId))
        .ToListAsync();

    return Results.Ok(participants);
});

events.MapPost("/{eventId:int}/participants/{userId:int}", async (int eventId, int userId, AppDbContext db) =>
{
    if (!await db.Events.AnyAsync(e => e.Id == eventId))
        return Results.NotFound("Event not found.");

    if (!await db.Users.AnyAsync(u => u.Id == userId))
        return Results.NotFound("User not found.");

    if (await db.EventParticipants.AnyAsync(ep => ep.EventId == eventId && ep.UserId == userId))
        return Results.Conflict("User is already a participant.");

    db.EventParticipants.Add(new EventParticipant { EventId = eventId, UserId = userId });
    await db.SaveChangesAsync();
    return Results.Created($"/events/{eventId}/participants", new ParticipantDto(eventId, userId));
});

events.MapDelete("/{eventId:int}/participants/{userId:int}", async (int eventId, int userId, AppDbContext db) =>
{
    var participant = await db.EventParticipants.FindAsync(eventId, userId);
    if (participant is null) return Results.NotFound();
    db.EventParticipants.Remove(participant);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// ── DTOs & Requests ────────────────────────────────────────────────────────

record UserDto(int Id, string Name, string Email);
record EventDto(int Id, string Title, DateTime StartTime, DateTime EndTime, int OrganizerId);
record ParticipantDto(int EventId, int UserId);

record CreateUserRequest(string Name, string Email);
record UpdateUserRequest(string Name, string Email);
record CreateEventRequest(string Title, DateTime StartTime, DateTime EndTime, int OrganizerId);
record UpdateEventRequest(string Title, DateTime StartTime, DateTime EndTime, int OrganizerId);
