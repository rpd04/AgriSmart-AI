import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Register() {
  const [form, setForm] = useState({ name: "", email: "", password: "", region: "", role: "Farmer" });
  const [error, setError] = useState("");
  const { register } = useAuth();
  const navigate = useNavigate();

  const update = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    try {
      await register(form);
      navigate("/");
    } catch (err) {
      setError(err.response?.data?.message || "Registration failed.");
    }
  };

  return (
    <div className="card auth-card">
      <h2>Create an account</h2>
      <form onSubmit={handleSubmit}>
        <label>Full name</label>
        <input value={form.name} onChange={update("name")} required />
        <label>Email</label>
        <input type="email" value={form.email} onChange={update("email")} required />
        <label>Password</label>
        <input type="password" value={form.password} onChange={update("password")} required minLength={6} />
        <label>Region</label>
        <input value={form.region} onChange={update("region")} placeholder="e.g. Kanpur, UP" />
        <label>Role</label>
        <select value={form.role} onChange={update("role")}>
          <option value="Farmer">Farmer</option>
          <option value="Agronomist">Agronomist</option>
          <option value="Admin">Admin</option>
        </select>
        {error && <p className="error">{error}</p>}
        <button type="submit">Register</button>
      </form>
      <p>Already have an account? <Link to="/login">Log in</Link></p>
    </div>
  );
}
