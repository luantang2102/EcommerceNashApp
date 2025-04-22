import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithErrorHandling } from "./baseApi";
import { ApiResponse } from "../models/apiResponse";
import { AuthResponse } from "../models/auth/auth";

export const authApi = createApi({
  reducerPath: "authApi",
  baseQuery: baseQueryWithErrorHandling,
  endpoints: (builder) => ({
    login: builder.mutation<AuthResponse, FormData>({
      query: (formData) => ({
        url: "auth/login",
        method: "POST",
        body: formData,
      }),
      transformResponse: (response: ApiResponse<AuthResponse>) => {
        return response.body;
      },
    }),

    register: builder.mutation<AuthResponse, FormData>({
      query: (formData) => ({
        url: "auth/register",
        method: "POST",
        body: formData,
      }),
      transformResponse: (response: ApiResponse<AuthResponse>) => {
        return response.body;
      },
    }),

    refreshToken: builder.query<AuthResponse, void>({
      query: () => ({
        url: "auth/refresh-token",
        method: "GET",
      }),
      transformResponse: (response: ApiResponse<AuthResponse>) => {
        return response.body;
      },
    }),

    checkAuth: builder.query<AuthResponse, void>({
      query: () => ({
        url: "auth/check",
        method: "GET",
      }),
      transformResponse: (response: ApiResponse<AuthResponse>) => {
        return response.body;
      },
    }),

    logout: builder.mutation<string, void>({
      query: () => ({
        url: "auth/logout",
        method: "POST",
      }),
      transformResponse: (response: ApiResponse<string>) => {
        return response.body;
      },
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useRefreshTokenQuery,
  useCheckAuthQuery,
  useLogoutMutation,
} = authApi;