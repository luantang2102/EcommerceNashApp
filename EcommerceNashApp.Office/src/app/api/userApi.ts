import { createApi } from "@reduxjs/toolkit/query/react";
import { PaginationParams } from "../models/params/pagination";
import { baseQueryWithErrorHandling } from "./baseApi";
import { ApiResponse } from "../models/apiResponse";
import { UserParams } from "../models/params/userParams";
import { User } from "../models/user";

export const userApi = createApi({
  reducerPath: "userApi",
  baseQuery: baseQueryWithErrorHandling,
  tagTypes: ["Users"],
  endpoints: (builder) => ({
    fetchUsers: builder.query<{ items: User[]; pagination: PaginationParams | null }, UserParams>({
      query: (userParams) => ({
        url: "users",
        params: userParams,
      }),
      transformResponse: (response: ApiResponse<User[]>, meta) => {
        const paginationHeader = meta?.response?.headers.get("Pagination");
        const pagination = paginationHeader ? JSON.parse(paginationHeader) as PaginationParams : null;

        return {
          items: response.body,
          pagination,
        };
      },
      providesTags: ["Users"],
    }),

    fetchUserById: builder.query<User, string>({
      query: (userId) => `users/${userId}`,
      transformResponse: (response: ApiResponse<User>) => {
        return response.body;
      },
      providesTags: ["Users"],
    }),
  }),
});

export const {
  useFetchUsersQuery,
  useFetchUserByIdQuery,
} = userApi;