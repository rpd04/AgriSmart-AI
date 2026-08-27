using System.ComponentModel.DataAnnotations;

namespace AgriSmart.API.Dtos;

public record CreateFarmRequest(
    [Required] string Location,
    [Range(0.1, 100000)] decimal AreaInAcres,
    string? SoilType
);

public record FarmResponse(int FarmId, string Location, decimal AreaInAcres, string? SoilType);

public record CreateCropRecordRequest(
    [Required] int FarmId,
    [Required] string CropType,
    [Required] DateTime SowingDate
);

public record CropRecordResponse(int CropRecordId, int FarmId, string CropType, DateTime SowingDate, string GrowthStage);
