namespace RaceDayAPI.Models;

public class Weather
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Condition { get; set; } = string.Empty;

    public double Temperature { get; set; }

    public double WindSpeed { get; set; }

    public int Humidity { get; set; }

    public Event? Event { get; set; }
}