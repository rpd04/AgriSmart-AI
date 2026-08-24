namespace AgriSmart.Core.Entities;

public class CropRecord
{
    public int CropRecordId { get; set; }
    public int FarmId { get; set; }
    public Farm? Farm { get; set; }

    public string CropType { get; set; } = string.Empty;
    public DateTime SowingDate { get; set; }
    public string GrowthStage { get; set; } = "Sown";

    public ICollection<DiseaseReport> DiseaseReports { get; set; } = new List<DiseaseReport>();
    public ICollection<YieldPrediction> YieldPredictions { get; set; } = new List<YieldPrediction>();
}
