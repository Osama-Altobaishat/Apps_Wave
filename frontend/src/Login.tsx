import { useState } from "react";
import axios from "axios";
import React from "react";
import "bootstrap/dist/css/bootstrap.min.css";
  import { useNavigate } from "react-router-dom";


export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const res = await axios.post("https://localhost:7142/api/auth/login", {
        email,
        password,
      });

      localStorage.setItem("accessToken", res.data.accessToken);
      localStorage.setItem("refreshToken", res.data.refreshToken);
      localStorage.setItem("role", res.data.role);

      if (res.data.role === 1) {
        window.location.href = "/dashboard";
      } else if (res.data.role === 0) {
        window.location.href = "/UserBookings";
      }
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        if (err.response?.status === 401) {
          setError("Email or password is incorrect");
        } else if (err.response?.status === 403) {
          setError("Account is inactive");
        } else {
          setError("Server error");
        }
      } else {
        setError("Unexpected error");
      }
    } finally {
      setLoading(false);
    }
  };


  return (
<div className="container d-flex justify-content-center align-items-center min-vh-100">
  <div className="card shadow-lg p-4" style={{ width: "400px" }}>
    
    <h3 className="text-center mb-4">Login</h3>

    {error && (
      <div className="alert alert-danger py-2 text-center">
        {error}
      </div>
    )}

    <form onSubmit={handleLogin}>
      
      {/* Email */}
      <div className="mb-3">
        <label className="form-label">Email</label>
        <input
          className="form-control"
          type="email"
          placeholder="Enter email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
      </div>

      {/* Password */}
      <div className="mb-3">
        <label className="form-label">Password</label>
        <input
          className="form-control"
          type="password"
          placeholder="Enter password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
      </div>

      {/* Login Button */}
      <button
        type="submit"
        className="btn btn-primary w-100"
        disabled={loading}
      >
        {loading ? "Loading..." : "Login"}
      </button>

      {/* Register Button */}
      <button
        type="button"
        className="btn btn-outline-secondary w-100 mt-2"
        onClick={() => navigate("/register")}
      >
        Create Account
      </button>

    </form>
  </div>
</div>
  );
}
