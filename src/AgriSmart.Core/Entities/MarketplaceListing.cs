namespace AgriSmart.Core.Entities;

public class MarketplaceListing
{
    public int ListingId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string CropType { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal SuggestedPrice { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
