import { configureStore } from "@reduxjs/toolkit";
import { productApi } from "../api/productApi";
import productReducer from "../../features/Products/productsSlice";
import categoryReducer from "../../features/Categories/categoriesSlice";
import { useDispatch, useSelector } from "react-redux";
import { uiSlice } from "../layout/uiSlice";
import { categoryApi } from "../api/categoryApi";

export const store = configureStore({
  reducer: {
    ui: uiSlice.reducer,
    [productApi.reducerPath]: productApi.reducer,
    [categoryApi.reducerPath]: categoryApi.reducer,
    product: productReducer,
    category: categoryReducer
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat([
      productApi.middleware,
      categoryApi.middleware
    ]),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export const useAppDispatch = useDispatch.withTypes<AppDispatch>();
export const useAppSelector = useSelector.withTypes<RootState>(); 

export default store;