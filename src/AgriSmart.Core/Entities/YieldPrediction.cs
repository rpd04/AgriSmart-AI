namespace AgriSmart.Core.Entities;

public class YieldPrediction
{
    public int PredictionId { get; set; }
    public int CropRecordId { get; set; }
    public CropRecord? CropRecord { get; set; }

    public double PredictedYieldKg { get; set; }
    public string LimitingFactor { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}
