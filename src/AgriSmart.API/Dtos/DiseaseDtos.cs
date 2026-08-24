namespace AgriSmart.API.Dtos;

public record DiseaseReportResponse(
    int ReportId,
    int CropRecordId,
    string PredictedDisease,
    double ConfidenceScore,
    string TreatmentAdvice,
    DateTime ReportedAt
);
