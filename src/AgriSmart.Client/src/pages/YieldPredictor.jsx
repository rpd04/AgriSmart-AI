import { useState } from "react";
import api from "../api/client";

const initialForm = {
  cropRecordId: "", nitrogen: 70, phosphorus: 40, potassium: 100,
  ph: 6.5, moisture: 30, rainfall: 120, temperature: 26,
};

export default function YieldPredictor() {
  const [form, setForm] = useState(initialForm);
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const update = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setResult(null);
    setLoading(true);
    try {
      const { data } = await api.post("/api/yield/predict", {
        ...form,
        cropRecordId: Number(form.cropRecordId),
        nitrogen: Number(form.nitrogen), phosphorus: Number(form.phosphorus), potassium: Number(form.potassium),
        ph: Number(form.ph), moisture: Number(form.moisture), rainfall: Number(form.rainfall), temperature: Number(form.temperature),
      });
      setResult(data);
    } catch (err) {
      setError(err.response?.data?.message || "Prediction failed. Is the AI service running?");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card">
      <h2>📈 Yield Predictor</h2>
      <form onSubmit={handleSubmit} className="form-grid">
        <div><label>Crop record ID</label><input value={form.cropRecordId} onChange={update("cropRecordId")} required /></div>
        <div><label>Nitrogen (ppm)</label><input type="number" value={form.nitrogen} onChange={update("nitrogen")} /></div>
        <div><label>Phosphorus (ppm)</label><input type="number" value={form.phosphorus} onChange={update("phosphorus")} /></div>
        <div><label>Potassium (ppm)</label><input type="number" value={form.potassium} onChange={update("potassium")} /></div>
        <div><label>Soil pH</label><input type="number" step="0.1" value={form.ph} onChange={update("ph")} /></div>
        <div><label>Moisture (%)</label><input type="number" value={form.moisture} onChange={update("moisture")} /></div>
        <div><label>Rainfall (mm)</label><input type="number" value={form.rainfall} onChange={update("rainfall")} /></div>
        <div><label>Temperature (°C)</label><input type="number" value={form.temperature} onChange={update("temperature")} /></div>
        <button type="submit" disabled={loading}>{loading ? "Predicting..." : "Predict yield"}</button>
      </form>
      {error && <p className="error">{error}</p>}
      {result && (
        <div className="result-box">
          <h3>{result.predictedYieldKg.toFixed(0)} kg/acre</h3>
          <p>Limiting factor: {result.limitingFactor}</p>
          <p className="muted">Model: {result.modelVersion}</p>
        </div>
      )}
    </div>
  );
}
