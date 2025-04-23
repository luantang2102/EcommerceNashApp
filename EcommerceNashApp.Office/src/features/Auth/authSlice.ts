import { createSlice } from "@reduxjs/toolkit";
import { authApi } from "../../app/api/authApi";
import { User } from "../../app/models/user";

interface AuthState {
  isAuthenticated: boolean;
  user: null | User;
  loading: boolean;
}

const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  loading: true,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setAuth: (state, action) => {
      state.isAuthenticated = true;
      state.user = action.payload;
      state.loading = false;
    },
    clearAuth: (state) => {
      state.isAuthenticated = false;
      state.user = null;
      state.loading = false;
    },
  },
  extraReducers: (builder) => {
    // Handle checkAuth query
    builder.addMatcher(
      authApi.endpoints.checkAuth.matchFulfilled,
      (state, { payload }) => {
        state.isAuthenticated = true;
        state.user = payload.user;
        state.loading = false;
      }
    );
    builder.addMatcher(authApi.endpoints.checkAuth.matchRejected, (state) => {
      state.isAuthenticated = false;
      state.user = null;
      state.loading = false;
    });
    // Handle login
    builder.addMatcher(authApi.endpoints.login.matchFulfilled, (state, { payload }) => {
      if (payload.user.roles.includes("Admin")) {
        state.isAuthenticated = true;
        state.user = payload.user;
        state.loading = false;
      } else {
        state.isAuthenticated = false;
        state.user = null;
        state.loading = false;
      }
    });
    // Handle register
    builder.addMatcher(authApi.endpoints.register.matchFulfilled, (state, { payload }) => {
      if (payload.user.roles.includes("Admin")) {
        state.isAuthenticated = true;
        state.user = payload.user;
        state.loading = false;
      } else {
        state.isAuthenticated = false;
        state.user = null;
        state.loading = false;
      }
    });
    // Handle logout
    builder.addMatcher(authApi.endpoints.logout.matchFulfilled, (state) => {
      state.isAuthenticated = false;
      state.user = null;
      state.loading = false;
    });
  },
});

export const { setAuth, clearAuth } = authSlice.actions;
export default authSlice.reducer;