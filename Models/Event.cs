namespace RaceDayAPI.Models;

public class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public string Location { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public double DistanceKm { get; set; }

    public string Status { get; set; } = string.Empty;

    public int OrganiserId { get; set; }

    public User? Organiser { get; set; }
}