namespace BookingService.Api.Core.Application.Features.BlockedTimes.DTOs;

public class BlockedTimeDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateBlockedTimeRequest
{
    public int ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}
