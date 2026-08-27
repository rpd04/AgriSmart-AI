using AgriSmart.API.Data;
using AgriSmart.API.Dtos;
using AgriSmart.API.Services;
using AgriSmart.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriSmart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class YieldController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAiServiceClient _ai;

    public YieldController(AppDbContext db, IAiServiceClient ai)
    {
        _db = db;
        _ai = ai;
    }

    [HttpPost("predict")]
    public async Task<ActionResult<YieldPredictionResponse>> Predict(YieldPredictionRequest request)
    {
        var cropRecord = await _db.CropRecords.FirstOrDefaultAsync(c => c.CropRecordId == request.CropRecordId);
        if (cropRecord is null) return NotFound(new { message = "Crop record not found." });

        var payload = new
        {
            nitrogen = request.Nitrogen,
            phosphorus = request.Phosphorus,
            potassium = request.Potassium,
            ph = request.Ph,
            moisture = request.Moisture,
            rainfall = request.Rainfall,
            temperature = request.Temperature,
        };
        var result = await _ai.PredictYieldAsync(payload);

        var prediction = new YieldPrediction
        {
            CropRecordId = request.CropRecordId,
            PredictedYieldKg = result.PredictedYieldKgPerAcre,
            LimitingFactor = result.LimitingFactor,
            ModelVersion = result.ModelVersion,
        };
        _db.YieldPredictions.Add(prediction);
        await _db.SaveChangesAsync();

        return Ok(new YieldPredictionResponse(
            prediction.Id, prediction.CropRecordId, prediction.PredictedYieldKg,
            prediction.LimitingFactor, prediction.ModelVersion, prediction.PredictedAt));
    }

    [HttpGet("crop-record/{cropRecordId:int}")]
    public async Task<ActionResult<List<YieldPredictionResponse>>> GetPredictionsForCrop(int cropRecordId)
    {
        var predictions = await _db.YieldPredictions
            .Where(p => p.CropRecordId == cropRecordId)
            .OrderByDescending(p => p.PredictedAt)
            .Select(p => new YieldPredictionResponse(p.Id, p.CropRecordId, p.PredictedYieldKg, p.LimitingFactor, p.ModelVersion, p.PredictedAt))
            .ToListAsync();
        return Ok(predictions);
    }
}
