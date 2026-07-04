import { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

export default function Register() {
  const navigate = useNavigate();

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  

 const register = async (e: React.FormEvent) => {
  e.preventDefault();

  setLoading(true);
  setError("");
  setMessage("");

  try {
    if (password !== confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    await axios.post("https://localhost:7142/api/auth/register", {
      fullName,
      email,
      password
    });

    setMessage("Account created successfully.");
    navigate("/");
  } catch (err: any) {
    if (err.response) {
      setError(err.response.data.message ?? "Registration failed.");
    } else {
      setError("Cannot connect to the server.");
    }
  } finally {
    setLoading(false);
  }
};

  return (
   <div className="container d-flex justify-content-center align-items-center min-vh-100">
  <div className="card shadow-lg p-4" style={{ width: "420px" }}>

    <h3 className="text-center mb-4">Register</h3>

    {message && (
      <div className="alert alert-success py-2 text-center">
        {message}
      </div>
    )}

    {error && (
      <div className="alert alert-danger py-2 text-center">
        {error}
      </div>
    )}

    <form onSubmit={register}>

      {/* Full Name */}
      <div className="mb-3">
        <label className="form-label">Full Name</label>
        <input
          type="text"
          className="form-control"
          value={fullName}
          onChange={(e) => setFullName(e.target.value)}
          required
        />
      </div>

      {/* Email */}
      <div className="mb-3">
        <label className="form-label">Email</label>
        <input
          type="email"
          className="form-control"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
      </div>

      {/* Password */}
      <div className="mb-3">
        <label className="form-label">Password</label>
        <input
          type="password"
          className="form-control"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </div>

      {/* Confirm Password */}
      <div className="mb-3">
        <label className="form-label">Confirm Password</label>
        <input
          type="password"
          className="form-control"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          required
        />
      </div>

      {/* Password mismatch warning */}
      {confirmPassword && password !== confirmPassword && (
        <div className="alert alert-warning py-2 text-center">
          Passwords do not match
        </div>
      )}

      {/* Submit */}
      <button
        className="btn btn-primary w-100"
        disabled={loading || password !== confirmPassword}
      >
        {loading ? "Creating..." : "Register"}
      </button>

    </form>
  </div>
</div>
  );
}