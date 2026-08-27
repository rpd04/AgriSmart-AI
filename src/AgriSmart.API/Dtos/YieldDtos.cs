using System.ComponentModel.DataAnnotations;

namespace AgriSmart.API.Dtos;

public record YieldPredictionRequest(
    [Required] int CropRecordId,
    [Range(0, 300)] double Nitrogen,
    [Range(0, 300)] double Phosphorus,
    [Range(0, 400)] double Potassium,
    [Range(0, 14)] double Ph,
    [Range(0, 100)] double Moisture,
    [Range(0, 1000)] double Rainfall,
    [Range(-10, 55)] double Temperature
);

public record YieldPredictionResponse(
    int PredictionId,
    int CropRecordId,
    double PredictedYieldKg,
    string LimitingFactor,
    string ModelVersion,
    DateTime PredictedAt
);
