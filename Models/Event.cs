namespace CtrlAltDeLorean.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int OrganizerId { get; set; }

    public User Organizer { get; set; } = null!;
    public ICollection<EventParticipant> Participants { get; set; } = [];
}
