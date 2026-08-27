namespace AgriSmart.Core.Entities;

public class DiseaseReport
{
    public int Id { get; set; }
    public int CropRecordId { get; set; }
    public CropRecord? CropRecord { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public string PredictedDisease { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string TreatmentAdvice { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
}
