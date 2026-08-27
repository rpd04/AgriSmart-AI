import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function NavBar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  return (
    <nav className="navbar">
      <div className="brand">🌾 AgriSmart AI</div>
      <div className="links">
        <Link to="/">Dashboard</Link>
        <Link to="/disease-scan">Disease Scan</Link>
        <Link to="/yield-predictor">Yield Predictor</Link>
        <Link to="/marketplace">Marketplace</Link>
        {user ? (
          <>
            <span className="user-pill">{user.name}</span>
            <button onClick={() => { logout(); navigate("/login"); }}>Logout</button>
          </>
        ) : (
          <Link to="/login">Login</Link>
        )}
      </div>
    </nav>
  );
}
