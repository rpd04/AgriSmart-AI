import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import api from "../api/client";

export default function Dashboard() {
  const { user } = useAuth();
  const [farms, setFarms] = useState([]);
  const [cropRecords, setCropRecords] = useState([]);
  const [newFarm, setNewFarm] = useState({ location: "", areaInAcres: "", soilType: "" });
  const [newCrop, setNewCrop] = useState({ farmId: "", cropType: "", sowingDate: "" });
  const [error, setError] = useState("");

  const loadData = async () => {
    if (!user) return;
    const [farmsRes, cropsRes] = await Promise.all([
      api.get("/api/farms"),
      api.get("/api/farms/crop-records"),
    ]);
    setFarms(farmsRes.data);
    setCropRecords(cropsRes.data);
  };

  useEffect(() => { loadData(); }, [user]);

  const addFarm = async (e) => {
    e.preventDefault();
    setError("");
    try {
      await api.post("/api/farms", newFarm);
      setNewFarm({ location: "", areaInAcres: "", soilType: "" });
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || "Could not add farm.");
    }
  };

  const addCrop = async (e) => {
    e.preventDefault();
    setError("");
    try {
      await api.post("/api/farms/crop-records", newCrop);
      setNewCrop({ farmId: "", cropType: "", sowingDate: "" });
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || "Could not add crop record.");
    }
  };

  if (!user) return <p className="card">Please log in to see your dashboard.</p>;

  return (
    <div className="grid-2">
      <div className="card">
        <h2>Your Farms</h2>
        <ul className="list">
          {farms.map((f) => (
            <li key={f.farmId}>{f.location} — {f.areaInAcres} acres {f.soilType ? `(${f.soilType})` : ""}</li>
          ))}
          {farms.length === 0 && <p className="muted">No farms yet — add one below.</p>}
        </ul>
        <form onSubmit={addFarm}>
          <label>Location</label>
          <input value={newFarm.location} onChange={(e) => setNewFarm({ ...newFarm, location: e.target.value })} required />
          <label>Area (acres)</label>
          <input type="number" step="0.1" value={newFarm.areaInAcres} onChange={(e) => setNewFarm({ ...newFarm, areaInAcres: e.target.value })} required />
          <label>Soil type</label>
          <input value={newFarm.soilType} onChange={(e) => setNewFarm({ ...newFarm, soilType: e.target.value })} />
          <button type="submit">Add farm</button>
        </form>
      </div>

      <div className="card">
        <h2>Crop Records</h2>
        <ul className="list">
          {cropRecords.map((c) => (
            <li key={c.cropRecordId}>#{c.cropRecordId} {c.cropType} — sown {new Date(c.sowingDate).toLocaleDateString()} ({c.growthStage})</li>
          ))}
          {cropRecords.length === 0 && <p className="muted">No crop records yet.</p>}
        </ul>
        <form onSubmit={addCrop}>
          <label>Farm</label>
          <select value={newCrop.farmId} onChange={(e) => setNewCrop({ ...newCrop, farmId: e.target.value })} required>
            <option value="">Select a farm</option>
            {farms.map((f) => <option key={f.farmId} value={f.farmId}>{f.location}</option>)}
          </select>
          <label>Crop type</label>
          <input value={newCrop.cropType} onChange={(e) => setNewCrop({ ...newCrop, cropType: e.target.value })} required placeholder="e.g. Wheat" />
          <label>Sowing date</label>
          <input type="date" value={newCrop.sowingDate} onChange={(e) => setNewCrop({ ...newCrop, sowingDate: e.target.value })} required />
          <button type="submit">Add crop record</button>
        </form>
      </div>
      {error && <p className="error">{error}</p>}
    </div>
  );
}
