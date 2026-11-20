namespace RaceDayAPI.Models;

public class Route
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string RouteName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double DistanceKm { get; set; }

    public string RouteMapUrl { get; set; } = string.Empty;

    public Event? Event { get; set; }
}