namespace AgriSmart.Core.Entities;

public class SoilData
{
    public int SoilDataId { get; set; }
    public int FarmId { get; set; }
    public Farm? Farm { get; set; }

    public double NitrogenPpm { get; set; }
    public double PhosphorusPpm { get; set; }
    public double PotassiumPpm { get; set; }
    public double Ph { get; set; }
    public double MoisturePercent { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
