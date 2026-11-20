

namespace RaceDayMVC1.Models;

public class ResultViewModel
{
    public int Id { get; set; }

    public int EnrolmentId { get; set; }

    public int EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public string ParticipantName { get; set; } = string.Empty;

    public TimeSpan FinishTime { get; set; }

    public int Position { get; set; }

    public string ResultStatus { get; set; } = "Finished";
}
