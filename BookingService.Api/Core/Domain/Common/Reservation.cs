using BookingService.Api.Core.Domain.Enums;

namespace BookingService.Api.Core.Domain.Common;

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Resource Resource { get; set; } = null!;

    // Business logic
    public TimeSpan Duration => EndTime - StartTime;

    public bool IsValid()
    {
        return EndTime > StartTime && 
        Duration >= TimeSpan.FromMinutes(30) && 
   Duration <= TimeSpan.FromHours(4);
    }

    public bool OverlapsWith(DateTime start, DateTime end)
    {
        return StartTime < end && EndTime > start;
    }
}
