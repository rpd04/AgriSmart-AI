using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgriSmart.API.Services;

public record YieldPredictionResult(double PredictedYieldKgPerAcre, string LimitingFactor, string ModelVersion);
public record DiseaseDetectionResult(string PredictedDisease, double Confidence, string TreatmentAdvice, string ModelVersion);

public interface IAiServiceClient
{
    Task<YieldPredictionResult> PredictYieldAsync(object payload, CancellationToken ct = default);
    Task<DiseaseDetectionResult> PredictDiseaseAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default);
}

/// <summary>
/// Thin wrapper around the Python FastAPI AI microservice (see ai-service/app.py).
/// Base URL comes from configuration ("AiService:BaseUrl"), e.g. http://localhost:8000.
/// </summary>
public class AiServiceClient : IAiServiceClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public AiServiceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<YieldPredictionResult> PredictYieldAsync(object payload, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/predict-yield", payload, JsonOpts, ct);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<YieldServiceResponse>(JsonOpts, ct)
            ?? throw new InvalidOperationException("Empty response from AI service.");
        return new YieldPredictionResult(body.PredictedYieldKgPerAcre, body.LimitingFactor, body.ModelVersion);
    }

    public async Task<DiseaseDetectionResult> PredictDiseaseAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(imageStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync("/predict-disease", content, ct);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<DiseaseServiceResponse>(JsonOpts, ct)
            ?? throw new InvalidOperationException("Empty response from AI service.");
        return new DiseaseDetectionResult(body.PredictedDisease, body.Confidence, body.TreatmentAdvice, body.ModelVersion);
    }

    // The Python AI service returns snake_case JSON (FastAPI/Pydantic default),
    // but JsonSerializerDefaults.Web expects camelCase — these attributes make
    // deserialization match the real field names instead of silently binding
    // everything to default values (null / 0).
    private record YieldServiceResponse(
        [property: JsonPropertyName("predicted_yield_kg_per_acre")] double PredictedYieldKgPerAcre,
        [property: JsonPropertyName("limiting_factor")] string LimitingFactor,
        [property: JsonPropertyName("model_version")] string ModelVersion);

    private record DiseaseServiceResponse(
        [property: JsonPropertyName("predicted_disease")] string PredictedDisease,
        [property: JsonPropertyName("confidence")] double Confidence,
        [property: JsonPropertyName("treatment_advice")] string TreatmentAdvice,
        [property: JsonPropertyName("model_version")] string ModelVersion);
}
