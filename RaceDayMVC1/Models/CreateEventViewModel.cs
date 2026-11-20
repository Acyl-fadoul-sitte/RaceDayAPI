
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC1.Models;

public class CreateEventViewModel
{
    [Required(ErrorMessage = "Event name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required.")]
    [DataType(DataType.Date)]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Location is required.")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an event type.")]
    public string EventType { get; set; } = string.Empty;

    [Range(0.1, 1000, ErrorMessage = "Distance must be greater than zero.")]
    public decimal DistanceKm { get; set; }
}
