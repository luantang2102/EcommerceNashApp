import { createBrowserRouter, Navigate } from "react-router-dom";
import App from "../layout/App";
import ServerError from "../errors/ServerError";
import NotFound from "../errors/NotFound";
import Dashboard from "../../features/Dashboard";
import Products from "../../features/Products";

export const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      {
        path: '',
        element: <Navigate replace to='/dashboard' />
      },
      {
        path: 'dashboard',
        element: <Dashboard />
      },
      {
        path: 'products',
        element: <Products />
      },
      {
        path: '/server-error',
        element: <ServerError />
      },
      {
        path: '/not-found',
        element: <NotFound />
      },
      {
        path: '/*',
        element: <Navigate replace to='/not-found' />
      },
    ]
  }
])