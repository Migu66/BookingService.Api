using BookingService.Api.Core.Domain.Enums;

namespace BookingService.Api.Core.Domain.Common;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
