namespace CtrlAltDeLorean.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Event> OrganizedEvents { get; set; } = [];
    public ICollection<EventParticipant> EventParticipants { get; set; } = [];
}
