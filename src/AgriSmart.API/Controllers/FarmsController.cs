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
[Authorize]
public class FarmsController : ControllerBase
{
    private readonly AppDbContext _db;

    public FarmsController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<List<FarmResponse>>> GetMyFarms()
    {
        var farms = await _db.Farms.Where(f => f.UserId == CurrentUserId)
            .Select(f => new FarmResponse(f.FarmId, f.Location, f.AreaInAcres, f.SoilType))
            .ToListAsync();
        return Ok(farms);
    }

    [HttpPost]
    public async Task<ActionResult<FarmResponse>> CreateFarm(CreateFarmRequest request)
    {
        var farm = new Farm
        {
            UserId = CurrentUserId,
            Location = request.Location,
            AreaInAcres = request.AreaInAcres,
            SoilType = request.SoilType,
        };
        _db.Farms.Add(farm);
        await _db.SaveChangesAsync();
        return Ok(new FarmResponse(farm.FarmId, farm.Location, farm.AreaInAcres, farm.SoilType));
    }

    [HttpPost("crop-records")]
    public async Task<ActionResult<CropRecordResponse>> AddCropRecord(CreateCropRecordRequest request)
    {
        var farm = await _db.Farms.FirstOrDefaultAsync(f => f.FarmId == request.FarmId && f.UserId == CurrentUserId);
        if (farm is null) return NotFound(new { message = "Farm not found." });

        var record = new CropRecord
        {
            FarmId = farm.FarmId,
            CropType = request.CropType,
            SowingDate = request.SowingDate,
            GrowthStage = "Sown",
        };
        _db.CropRecords.Add(record);
        await _db.SaveChangesAsync();
        return Ok(new CropRecordResponse(record.CropRecordId, record.FarmId, record.CropType, record.SowingDate, record.GrowthStage));
    }

    [HttpGet("crop-records")]
    public async Task<ActionResult<List<CropRecordResponse>>> GetMyCropRecords()
    {
        var records = await _db.CropRecords
            .Where(c => c.Farm!.UserId == CurrentUserId)
            .Select(c => new CropRecordResponse(c.CropRecordId, c.FarmId, c.CropType, c.SowingDate, c.GrowthStage))
            .ToListAsync();
        return Ok(records);
    }
}
