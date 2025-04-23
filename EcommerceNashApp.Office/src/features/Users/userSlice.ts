import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { UserParams } from "../../app/models/params/userParams";

interface UserState {
  params: UserParams;
  selectedUserId: string | null;
  isCreateFormOpen: boolean;
  isDeleteDialogOpen: boolean;
}

const initialState: UserState = {
  params: { pageNumber: 1, pageSize: 10 },
  selectedUserId: null,
  isCreateFormOpen: false,
  isDeleteDialogOpen: false,
};

const userSlice = createSlice({
  name: "user",
  initialState,
  reducers: {
    setParams: (state, action: PayloadAction<Partial<UserParams>>) => {
      state.params = { ...state.params, ...action.payload, pageNumber: 1 };
    },
    setPageNumber: (state, action: PayloadAction<number>) => {
      state.params.pageNumber = action.payload;
    },
    setSelectedUserId: (state, action: PayloadAction<string | null>) => {
      state.selectedUserId = action.payload;
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
  setSelectedUserId,
  setCreateFormOpen,
  setDeleteDialogOpen,
  resetParams,
} = userSlice.actions;

export default userSlice.reducer;