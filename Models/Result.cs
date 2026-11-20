namespace RaceDayAPI.Models;

public class Result
{
    public int Id { get; set; }

    public int EnrolmentId { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int Position { get; set; }

    public string ResultStatus { get; set; } = string.Empty;

    public Enrolment? Enrolment { get; set; }
}