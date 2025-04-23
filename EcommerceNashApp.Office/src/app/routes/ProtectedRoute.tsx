// routes/ProtectedRoute.tsx
import { Navigate } from "react-router-dom";
import { useAppSelector } from "../store/store";
import { JSX } from "react";
import { CircularProgress } from "@mui/material";

interface ProtectedRouteProps {
  children: JSX.Element;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated: isAuthenticated, loading } = useAppSelector((state) => state.auth);

  if (loading) {
    return <div><CircularProgress></CircularProgress></div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/signin" replace />;
  }

  return children;
};

export default ProtectedRoute;