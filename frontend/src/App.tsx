import { BrowserRouter, Routes, Route } from "react-router-dom";
import Login from "./Login";
import UserBookings from "./UserBookings";
import ProtectedRoute from "./ProtectedRoute";
import Dashboard from "./Dashboard";
import Register from "./Register";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} />

        <Route
          path="/UserBookings"
          element={
            <ProtectedRoute role="0">
              <UserBookings />
            </ProtectedRoute>
          }
        />

        <Route
          path="/dashboard"
          element={
            <ProtectedRoute role="1">
              <Dashboard />
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}
