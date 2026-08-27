using System.ComponentModel.DataAnnotations;

namespace AgriSmart.API.Dtos;

public record CreateListingRequest(
    [Required] string CropType,
    [Range(0.1, 1000000)] decimal QuantityKg,
    decimal? AskingPrice
);

public record ListingResponse(
    int ListingId,
    string CropType,
    decimal QuantityKg,
    decimal SuggestedPrice,
    string Status,
    DateTime CreatedAt,
    string SellerName
);
