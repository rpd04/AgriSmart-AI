import { useState } from "react";
import api from "../api/client";

export default function DiseaseScan() {
  const [cropRecordId, setCropRecordId] = useState("");
  const [file, setFile] = useState(null);
  const [preview, setPreview] = useState(null);
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const onFileChange = (e) => {
    const f = e.target.files[0];
    setFile(f);
    setPreview(f ? URL.createObjectURL(f) : null);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setResult(null);
    if (!file || !cropRecordId) {
      setError("Select a crop record and a leaf image first.");
      return;
    }
    setLoading(true);
    try {
      const formData = new FormData();
      formData.append("file", file);
      const { data } = await api.post(`/api/disease/scan/${cropRecordId}`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      setResult(data);
    } catch (err) {
      setError(err.response?.data?.message || "Scan failed. Is the AI service running?");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card">
      <h2>🔬 Crop Disease Scan</h2>
      <p className="muted">Upload a leaf photo to get an instant diagnosis and treatment advice.</p>
      <form onSubmit={handleSubmit}>
        <label>Crop record ID</label>
        <input value={cropRecordId} onChange={(e) => setCropRecordId(e.target.value)} placeholder="e.g. 1" required />
        <label>Leaf photo</label>
        <input type="file" accept="image/*" onChange={onFileChange} required />
        {preview && <img src={preview} alt="preview" className="preview" />}
        <button type="submit" disabled={loading}>{loading ? "Scanning..." : "Scan leaf"}</button>
      </form>
      {error && <p className="error">{error}</p>}
      {result && (
        <div className="result-box">
          <h3>{result.predictedDisease}</h3>
          <p>Confidence: {(result.confidenceScore * 100).toFixed(1)}%</p>
          <p>{result.treatmentAdvice}</p>
        </div>
      )}
    </div>
  );
}
