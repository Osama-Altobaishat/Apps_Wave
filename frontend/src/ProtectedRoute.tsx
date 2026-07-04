import { Navigate } from "react-router-dom";
import type { ReactElement } from "react";

type Props = {
  children: ReactElement;
  role?: string;
};

export default function ProtectedRoute({ children, role }: Props) {
  const token = localStorage.getItem("refreshToken");
  const userRole = localStorage.getItem("role");

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (role && userRole !== role) {
    return <Navigate to="/login" replace />;
  }


  return children;
}