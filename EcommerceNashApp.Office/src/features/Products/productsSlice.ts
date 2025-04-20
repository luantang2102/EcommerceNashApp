import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { ProductParams } from "../../app/models/params/productParams";

interface ProductState {
  params: ProductParams;
  selectedProductId: string | null;
  isCreateFormOpen: boolean;
  isDeleteDialogOpen: boolean;
}

const initialState: ProductState = {
  params: { pageNumber: 1, pageSize: 10 },
  selectedProductId: null,
  isCreateFormOpen: false,
  isDeleteDialogOpen: false,
};

const productSlice = createSlice({
  name: "product",
  initialState,
  reducers: {
    setParams: (state, action: PayloadAction<Partial<ProductParams>>) => {
      state.params = { ...state.params, ...action.payload, pageNumber: 1 };
    },
    setPageNumber: (state, action: PayloadAction<number>) => {
      state.params.pageNumber = action.payload;
    },
    setSelectedProductId: (state, action: PayloadAction<string | null>) => {
      state.selectedProductId = action.payload;
    },
    setCreateFormOpen: (state, action: PayloadAction<boolean>) => {
      state.isCreateFormOpen = action.payload;
    },
    setDeleteDialogOpen: (state, action: PayloadAction<boolean>) => {
      state.isDeleteDialogOpen = action.payload;
    },
    resetParams: (state) => {
      state.params = { pageNumber: 1, pageSize: 10 };
    },
  },
});

export const {
  setParams,
  setPageNumber,
  setSelectedProductId,
  setCreateFormOpen,
  setDeleteDialogOpen,
  resetParams,
} = productSlice.actions;

export default productSlice.reducer;