import { createApi } from "@reduxjs/toolkit/query/react";
import { Product } from "../models/product";
import { PaginationParams } from "../models/params/pagination";
import { ProductParams } from "../models/params/productParams";
import { baseQueryWithErrorHandling } from "./baseApi";
import { ApiResponse } from "../models/apiResponse";

export const productApi = createApi({
  reducerPath: "productApi",
  baseQuery: baseQueryWithErrorHandling,
  tagTypes: ["Products"],
  endpoints: (builder) => ({
    fetchProducts: builder.query<{ items: Product[]; pagination: PaginationParams | null }, ProductParams>({
      query: (productParams) => ({
        url: "products",
        params: productParams,
      }),
      transformResponse: (response: ApiResponse<Product[]>, meta) => {
        const paginationHeader = meta?.response?.headers.get("Pagination");
        const pagination = paginationHeader ? JSON.parse(paginationHeader) as PaginationParams : null;

        return {
          items: response.body,
          pagination,
        };
      },
      providesTags: ["Products"],
    }),

    fetchProductById: builder.query<Product, string>({
      query: (productId) => `products/${productId}`,
      transformResponse: (response: ApiResponse<Product>) => {
        return response.body;
      },
      providesTags: ["Products"],
    }),

    fetchProductsByCategory: builder.query<{ items: Product[]; pagination: PaginationParams | null }, { categoryId: string; params: ProductParams }>({
      query: ({ categoryId, params }) => ({
        url: `products/category/${categoryId}`,
        params,
      }),
      transformResponse: (response: ApiResponse<Product[]>, meta) => {
        const paginationHeader = meta?.response?.headers.get("Pagination");
        const pagination = paginationHeader ? JSON.parse(paginationHeader) as PaginationParams : null;

        return {
          items: response.body,
          pagination,
        };
      },
      providesTags: ["Products"],
    }),

    createProduct: builder.mutation<Product, FormData>({
      query: (data) => ({
        url: "products",
        method: "POST",
        body: data,
      }),
      invalidatesTags: ["Products"],
    }),

    updateProduct: builder.mutation<Product, { id: string; data: FormData }>({
      query: ({ id, data }) => ({
        url: `products/${id}`,
        method: "PUT",
        body: data,
      }),
      invalidatesTags: ["Products"],
    }),

    deleteProduct: builder.mutation<void, string>({
      query: (id) => ({
        url: `products/${id}`,
        method: "DELETE",
      }),
      invalidatesTags: ["Products"],
    }),
  }),
});

export const {
  useFetchProductsQuery,
  useFetchProductByIdQuery,
  useFetchProductsByCategoryQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
} = productApi;
