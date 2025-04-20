// src/store/store.ts
import { configureStore } from "@reduxjs/toolkit";
import { productApi } from "../api/productApi";
import productReducer from "../../features/Products/productsSlice";
import { useDispatch, useSelector } from "react-redux";
import { uiSlice } from "../layout/uiSlice";

export const store = configureStore({
  reducer: {
    ui: uiSlice.reducer,
    [productApi.reducerPath]: productApi.reducer,
    product: productReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(productApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export const useAppDispatch = useDispatch.withTypes<AppDispatch>();
export const useAppSelector = useSelector.withTypes<RootState>(); 

export default store;