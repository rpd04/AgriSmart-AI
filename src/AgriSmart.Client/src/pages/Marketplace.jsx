import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import api from "../api/client";

export default function Marketplace() {
  const { user } = useAuth();
  const [listings, setListings] = useState([]);
  const [form, setForm] = useState({ cropType: "", quantityKg: "", askingPrice: "" });
  const [error, setError] = useState("");

  const load = async () => {
    const { data } = await api.get("/api/marketplace");
    setListings(data);
  };

  useEffect(() => { load(); }, []);

  const submit = async (e) => {
    e.preventDefault();
    setError("");
    try {
      await api.post("/api/marketplace", {
        cropType: form.cropType,
        quantityKg: Number(form.quantityKg),
        askingPrice: form.askingPrice ? Number(form.askingPrice) : null,
      });
      setForm({ cropType: "", quantityKg: "", askingPrice: "" });
      load();
    } catch (err) {
      setError(err.response?.data?.message || "Could not create listing.");
    }
  };

  return (
    <div className="grid-2">
      <div className="card">
        <h2>🛒 Marketplace</h2>
        <ul className="list">
          {listings.map((l) => (
            <li key={l.listingId}>
              <strong>{l.cropType}</strong> — {l.quantityKg} kg @ ₹{l.suggestedPrice}/kg
              <span className="muted"> by {l.sellerName}</span>
            </li>
          ))}
          {listings.length === 0 && <p className="muted">No active listings yet.</p>}
        </ul>
      </div>
      <div className="card">
        <h2>List your produce</h2>
        {!user && <p className="muted">Log in to create a listing.</p>}
        {user && (
          <form onSubmit={submit}>
            <label>Crop type</label>
            <input value={form.cropType} onChange={(e) => setForm({ ...form, cropType: e.target.value })} required />
            <label>Quantity (kg)</label>
            <input type="number" value={form.quantityKg} onChange={(e) => setForm({ ...form, quantityKg: e.target.value })} required />
            <label>Your asking price per kg (optional — we'll suggest one if left blank)</label>
            <input type="number" value={form.askingPrice} onChange={(e) => setForm({ ...form, askingPrice: e.target.value })} />
            <button type="submit">Create listing</button>
          </form>
        )}
        {error && <p className="error">{error}</p>}
      </div>
    </div>
  );
}
