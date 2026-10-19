

using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models;

[Table("Enrolments")]
public class Enrolment
{
    [Column("EnrolmentId")]
    public int Id { get; set; }

    public int ParticipantId { get; set; }

    public int CategoryId { get; set; }

    public DateTime EnrolmentDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public User? Participant { get; set; }

    public Category? Category { get; set; }
}

