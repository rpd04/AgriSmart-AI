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
public class DiseaseController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAiServiceClient _ai;

    public DiseaseController(AppDbContext db, IAiServiceClient ai)
    {
        _db = db;
        _ai = ai;
    }

    /// <summary>
    /// Upload a leaf photo for a crop record; forwards it to the AI service and stores the result.
    /// </summary>
    [HttpPost("scan/{cropRecordId:int}")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<DiseaseReportResponse>> ScanLeaf(int cropRecordId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Please attach an image file." });

        var cropRecord = await _db.CropRecords.FirstOrDefaultAsync(c => c.CropRecordId == cropRecordId);
        if (cropRecord is null) return NotFound(new { message = "Crop record not found." });

        await using var stream = file.OpenReadStream();
        var result = await _ai.PredictDiseaseAsync(stream, file.FileName, file.ContentType);

        var report = new DiseaseReport
        {
            CropRecordId = cropRecordId,
            ImageUrl = file.FileName, // swap for a real Blob Storage URL in production
            PredictedDisease = result.PredictedDisease,
            ConfidenceScore = result.Confidence,
            TreatmentAdvice = result.TreatmentAdvice,
        };
        _db.DiseaseReports.Add(report);
        await _db.SaveChangesAsync();

        return Ok(new DiseaseReportResponse(
            report.Id, report.CropRecordId, report.PredictedDisease,
            report.ConfidenceScore, report.TreatmentAdvice, report.ReportedAt));
    }

    [HttpGet("crop-record/{cropRecordId:int}")]
    public async Task<ActionResult<List<DiseaseReportResponse>>> GetReportsForCrop(int cropRecordId)
    {
        var reports = await _db.DiseaseReports
            .Where(r => r.CropRecordId == cropRecordId)
            .OrderByDescending(r => r.ReportedAt)
            .Select(r => new DiseaseReportResponse(r.Id, r.CropRecordId, r.PredictedDisease, r.ConfidenceScore, r.TreatmentAdvice, r.ReportedAt))
            .ToListAsync();
        return Ok(reports);
    }
}
