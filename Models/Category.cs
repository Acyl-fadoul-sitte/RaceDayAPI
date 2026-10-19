
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models;

public class Category
{
    [Column("CategoryId")]
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal EntryFee { get; set; }

    [Column("MaximumParticipants")]
public int MaxParticipants { get; set; }

    public Event? Event { get; set; }
}
