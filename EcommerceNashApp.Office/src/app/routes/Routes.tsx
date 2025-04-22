import { createBrowserRouter, Navigate } from "react-router-dom";
import App from "../layout/App";
import ServerError from "../errors/ServerError";
import NotFound from "../errors/NotFound";
import Dashboard from "../../features/Dashboard";
import Products from "../../features/Products";
import Categories from "../../features/Categories";
import ProtectedRoute from "./ProtectedRoute";
import AuthLayout from "../layout/Auth";
import SignIn from "../../features/Auth/SignIn";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        path: "",
        element: <Navigate replace to="/dashboard" />,
      },
      {
        path: "dashboard",
        element: <ProtectedRoute><Dashboard /></ProtectedRoute>,
      },
      {
        path: "products",
        element: <ProtectedRoute><Products /></ProtectedRoute>,
      },
      {
        path: "categories",
        element: <ProtectedRoute><Categories /></ProtectedRoute>,
      },
      {
        path: "server-error",
        element: <ServerError />,
      },
      {
        path: "not-found",
        element: <NotFound />,
      },
      {
        path: "/*",
        element: <Navigate replace to="/not-found" />,
      },
    ],
  },
  {
    path: "/",
    element: <AuthLayout />,
    children: [
      {
        path: "signin",
        element: <SignIn />,
      }
    ],
  },
]);