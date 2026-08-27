namespace AgriSmart.Core.Entities;

public enum UserRole
{
    Farmer,
    Agronomist,
    Admin
}

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Region { get; set; }
    public UserRole Role { get; set; } = UserRole.Farmer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Farm> Farms { get; set; } = new List<Farm>();
    public ICollection<MarketplaceListing> Listings { get; set; } = new List<MarketplaceListing>();
}
