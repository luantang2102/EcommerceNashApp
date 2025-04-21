import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { CategoryParams } from "../../app/models/params/categoryParams";

interface CategoryState {
  params: CategoryParams;
  selectedCategoryId: string | null;
  isCreateFormOpen: boolean;
  isDeleteDialogOpen: boolean;
}

const initialState: CategoryState = {
  params: { pageNumber: 1, pageSize: 10 },
  selectedCategoryId: null,
  isCreateFormOpen: false,
  isDeleteDialogOpen: false,
};

const categorySlice = createSlice({
  name: "category",
  initialState,
  reducers: {
    setParams: (state, action: PayloadAction<Partial<CategoryParams>>) => {
      state.params = { ...state.params, ...action.payload, pageNumber: 1 };
    },
    setPageNumber: (state, action: PayloadAction<number>) => {
      state.params.pageNumber = action.payload;
    },
    setSelectedCategoryId: (state, action: PayloadAction<string | null>) => {
      state.selectedCategoryId = action.payload;
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
  setSelectedCategoryId,
  setCreateFormOpen,
  setDeleteDialogOpen,
  resetParams,
} = categorySlice.actions;

export default categorySlice.reducer;