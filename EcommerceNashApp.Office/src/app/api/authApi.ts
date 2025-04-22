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

    refreshToken: builder.mutation<AuthResponse, FormData>({
      query: (formData) => ({
        url: "auth/refresh-token",
        method: "POST",
        body: formData,
      }),
      transformResponse: (response: ApiResponse<AuthResponse>) => {
        return response.body;
      },
    }),

    logout: builder.mutation<void, void>({
      query: () => ({
        url: "auth/logout",
        method: "POST",
      }),
      transformResponse: () => {
        return undefined;
      },
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useRefreshTokenMutation,
  useLogoutMutation,
} = authApi;