import { createSlice } from "@reduxjs/toolkit";

const getInitDarkMode = () => {
  if (typeof window !== 'undefined') {
    const storedDarkMode = localStorage.getItem('darkMode');
    return storedDarkMode ? JSON.parse(storedDarkMode) : true;
  }
  return true;
};

export const uiSlice = createSlice({
  name: 'ui',
  initialState: {
    isLoading: false,
    darkMode: getInitDarkMode(),
    openSideBar: true,
    showCreateStoryModal: false,
    showCreateStatusModal: false
  },
  reducers: {
    setLoading: (state, action) => {
      state.isLoading = action.payload;
    },
    toggleDarkMode: (state) => {
      const newDarkMode = !state.darkMode;
      if (typeof window !== 'undefined') {
        localStorage.setItem('darkMode', JSON.stringify(newDarkMode));
      }
      state.darkMode = newDarkMode;
    },
    setSidebarOpen: (state, action) => {
      state.openSideBar = action.payload;
    },
    setCreateStoryModal: (state, action) => {
      state.showCreateStoryModal = action.payload;
    },
    setCreateStatusModal: (state, action) => {
      state.showCreateStatusModal = action.payload;
    }
  }
});

export const { 
  setLoading, 
  toggleDarkMode, 
  setSidebarOpen, 
  setCreateStoryModal ,
  setCreateStatusModal
} = uiSlice.actions;

export default uiSlice.reducer;