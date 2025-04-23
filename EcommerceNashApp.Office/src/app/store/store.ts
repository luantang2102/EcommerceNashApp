import { configureStore } from "@reduxjs/toolkit";
import { productApi } from "../api/productApi";
import { categoryApi } from "../api/categoryApi";
import { userApi } from "../api/userApi";
import { authApi } from "../api/authApi";
import productReducer from "../../features/Products/productsSlice";
import categoryReducer from "../../features/Categories/categoriesSlice";
import userReducer from "../../features/Users/userSlice";
import authReducer from "../../features/Auth/authSlice";
import { uiSlice } from "../layout/uiSlice";
import { useDispatch, useSelector } from "react-redux";

export const store = configureStore({
  reducer: {
    ui: uiSlice.reducer,
    [productApi.reducerPath]: productApi.reducer,
    [categoryApi.reducerPath]: categoryApi.reducer,
    [userApi.reducerPath]: userApi.reducer,
    [authApi.reducerPath]: authApi.reducer,
    product: productReducer,
    category: categoryReducer,
    user: userReducer,
    auth: authReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat([
      productApi.middleware,
      categoryApi.middleware,
      userApi.middleware,
      authApi.middleware,
    ]),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export const useAppDispatch = useDispatch.withTypes<AppDispatch>();
export const useAppSelector = useSelector.withTypes<RootState>();

export default store;