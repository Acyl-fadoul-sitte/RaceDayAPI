
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models;

public class Result
{
    [Column("ResultId")]
    public int Id { get; set; }

    public int EnrolmentId { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int Position { get; set; }

    [Column("Status")]
public string ResultStatus { get; set; } = string.Empty;

    public Enrolment? Enrolment { get; set; }
}
