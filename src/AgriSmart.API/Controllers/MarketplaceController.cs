using System.Security.Claims;
using AgriSmart.API.Data;
using AgriSmart.API.Dtos;
using AgriSmart.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriSmart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketplaceController : ControllerBase
{
    private readonly AppDbContext _db;

    public MarketplaceController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ListingResponse>>> GetActiveListings()
    {
        var listings = await _db.MarketplaceListings
            .Where(l => l.Status == "Active")
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new ListingResponse(l.ListingId, l.CropType, l.QuantityKg, l.SuggestedPrice, l.Status, l.CreatedAt, l.User!.Name))
            .ToListAsync();
        return Ok(listings);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ListingResponse>> CreateListing(CreateListingRequest request)
    {
        // naive fair-price suggestion: recent average price for the same crop, or the farmer's ask if none exists yet
        var recentAvg = await _db.MarketplaceListings
            .Where(l => l.CropType == request.CropType)
            .OrderByDescending(l => l.CreatedAt)
            .Take(10)
            .Select(l => l.SuggestedPrice)
            .ToListAsync();

        var suggestedPrice = request.AskingPrice
            ?? (recentAvg.Count > 0 ? recentAvg.Average() : 20m); // fallback default price/kg

        var listing = new MarketplaceListing
        {
            UserId = CurrentUserId,
            CropType = request.CropType,
            QuantityKg = request.QuantityKg,
            SuggestedPrice = suggestedPrice,
        };
        _db.MarketplaceListings.Add(listing);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(CurrentUserId);
        return Ok(new ListingResponse(listing.ListingId, listing.CropType, listing.QuantityKg, listing.SuggestedPrice, listing.Status, listing.CreatedAt, user?.Name ?? ""));
    }
}
