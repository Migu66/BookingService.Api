namespace BookingService.Api.Core.Domain.Common;

public class BlockedTime
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Resource Resource { get; set; } = null!;

    public bool OverlapsWith(DateTime start, DateTime end)
    {
        return StartTime < end && EndTime > start;
    }
}
