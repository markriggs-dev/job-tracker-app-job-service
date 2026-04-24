namespace JobService.Core.Enums;

public enum JobStatus
{
    Discovered,
    Applied,
    InProgress,
    WaitingOnResponse,
    InterviewScheduled,
    OfferReceived,
    Closed,
    Withdrawn
}
