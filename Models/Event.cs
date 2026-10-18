


using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models;

public class Event
{
    [Column("EventId")]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public string Location { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    [Column("Distance")]
    public decimal DistanceKm { get; set; }

    [Column("OrganiserId")]
    public int OrganiserId { get; set; }

    public User? Organiser { get; set; }
}
