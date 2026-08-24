namespace AgriSmart.Core.Entities;

public class Farm
{
    public int FarmId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Location { get; set; } = string.Empty;
    public decimal AreaInAcres { get; set; }
    public string? SoilType { get; set; }

    public ICollection<CropRecord> CropRecords { get; set; } = new List<CropRecord>();
    public ICollection<SoilData> SoilData { get; set; } = new List<SoilData>();
}
