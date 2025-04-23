import { useState, useEffect, useCallback } from "react";
import {
  Box,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Pagination,
  Button,
  Typography,
  Paper,
  Chip,
  InputAdornment,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Grid,
  CircularProgress,
  Alert,
  Snackbar,
  CardContent,
  Card,
  Divider,
  Tooltip,
} from "@mui/material";
import {
  Search as SearchIcon,
  Refresh as RefreshIcon,
  Clear as ClearIcon,
  ContentCopy,
} from "@mui/icons-material";
import { setParams, setPageNumber } from "./userSlice";
import { useAppDispatch, useAppSelector } from "../../app/store/store";
import { useFetchUsersQuery } from "../../app/api/userApi";
import { format } from "date-fns";
import { PaginationParams } from "../../app/models/params/pagination";
import { debounce } from "lodash";
import { UserProfile } from "../../app/models/userProfile";

export default function UserList() {
  const dispatch = useAppDispatch();
  const { params } = useAppSelector((state) => state.user);
  const { data, isLoading, error, refetch, isFetching } = useFetchUsersQuery(params);
  const [search, setSearch] = useState(params.searchTerm || "");
  const [selectedProfile, setSelectedProfile] = useState<UserProfile | null>(null);
  const [isProfileDialogOpen, setIsProfileDialogOpen] = useState(false);

  // Notification state
  const [notification, setNotification] = useState({
    open: false,
    message: "",
    severity: "success" as "success" | "error" | "info" | "warning",
  });

  // Debounced search handler
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const debouncedSearch = useCallback(
    debounce((value: string) => {
      dispatch(setParams({ searchTerm: value.trim() || undefined }));
      dispatch(setPageNumber(1)); // Reset to first page on new search
    }, 500),
    [dispatch]
  );

  // Sync search input with params.search
  useEffect(() => {
    setSearch(params.searchTerm || "");
  }, [params.searchTerm]);

  // Search and pagination
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearch(value);
    debouncedSearch(value);
  };

  const handleClearSearch = () => {
    setSearch("");
    dispatch(setParams({ searchTerm: undefined }));
    dispatch(setPageNumber(1));
  };

  const handlePageChange = (_: React.ChangeEvent<unknown>, page: number) => {
    dispatch(setPageNumber(page));
  };

  const handleCloseNotification = () => {
    setNotification({ ...notification, open: false });
  };

  // Profile click handler
  const handleProfileClick = (profile: UserProfile) => {
    setSelectedProfile(profile);
    setIsProfileDialogOpen(true);
  };

  const handleCloseProfileDialog = () => {
    setIsProfileDialogOpen(false);
    setSelectedProfile(null);
  };

  // Copy ID handler
  const handleCopyId = (id: string) => {
    navigator.clipboard.writeText(id).then(() => {
      setNotification({
        open: true,
        message: "User ID copied to clipboard",
        severity: "info",
      });
    }).catch((err) => {
      console.error("Failed to copy ID:", err);
      setNotification({
        open: true,
        message: "Failed to copy ID",
        severity: "error",
      });
    });
  };

  const formatDate = (dateString: string | null) => {
    if (!dateString) return "N/A";
    return format(new Date(dateString), "MMM dd, yyyy HH:mm");
  };

  const calculateStartIndex = (pagination: PaginationParams) => {
    return (pagination.currentPage - 1) * pagination.pageSize + 1;
  };

  const calculateEndIndex = (pagination: PaginationParams) => {
    const endIndex = pagination.currentPage * pagination.pageSize;
    return endIndex > pagination.totalCount ? pagination.totalCount : endIndex;
  };

  // Format user profiles for display
  const formatUserProfiles = (profiles: UserProfile[]) => {
    if (!profiles || profiles.length === 0) return "None";
    return profiles
      .map((profile, index) => (
        <Typography
          key={profile.id}
          component="span"
          sx={{
            color: "primary.main",
            cursor: "pointer",
            "&:hover": { textDecoration: "underline" },
            mr: profiles.length > 1 ? 0.5 : 0,
          }}
          onClick={() => handleProfileClick(profile)}
        >
          {`Profile ${index + 1}${index < profiles.length - 1 ? "," : ""}`}
        </Typography>
      ));
  };

  if (isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", p: 5 }}>
        <CircularProgress />
        <Typography variant="h6" sx={{ ml: 2 }}>
          Loading users...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Card sx={{ p: 3, m: 2, bgcolor: "#fff4f4" }}>
        <CardContent>
          <Typography variant="h6" color="error">
            Error loading users
          </Typography>
          <Typography variant="body2" sx={{ mt: 1 }}>
            Please try again later or contact support.
          </Typography>
          <Button
            startIcon={<RefreshIcon />}
            variant="outlined"
            color="primary"
            sx={{ mt: 2 }}
            onClick={() => refetch()}
          >
            Retry
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Notification */}
      <Snackbar
        open={notification.open}
        autoHideDuration={6000}
        onClose={handleCloseNotification}
        anchorOrigin={{ vertical: "top", horizontal: "right" }}
      >
        <Alert
          onClose={handleCloseNotification}
          severity={notification.severity}
          sx={{ width: "100%" }}
        >
          {notification.message}
        </Alert>
      </Snackbar>

      {/* Page Header */}
      <Typography variant="h5" sx={{ mb: 3, fontWeight: 500 }}>
        User Management
      </Typography>

      {/* Main Content */}
      <Paper elevation={2} sx={{ p: 3, mb: 4 }}>
        {/* Search */}
        <Box sx={{ display: "flex", justifyContent: "flex-start", mb: 3 }}>
          <TextField
            label="Search Users"
            value={search}
            onChange={handleSearchChange}
            variant="outlined"
            size="small"
            sx={{ width: "300px" }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
              endAdornment: search && (
                <InputAdornment position="end">
                  <IconButton size="small" onClick={handleClearSearch}>
                    <ClearIcon />
                  </IconButton>
                </InputAdornment>
              ),
            }}
            disabled={isFetching}
          />
        </Box>

        <Divider sx={{ mb: 2 }} />

        {/* Users Table */}
        <TableContainer component={Paper} elevation={0} sx={{ mb: 2 }}>
          <Table sx={{ minWidth: 650 }}>
            <TableHead>
              <TableRow>
                <TableCell>Username</TableCell>
                <TableCell>Email</TableCell>
                <TableCell>Roles</TableCell>
                <TableCell>User Profiles</TableCell>
                <TableCell>Created Date</TableCell>
                <TableCell align="center">Action</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.items.map((user) => (
                <TableRow
                  key={user.id}
                >
                  <TableCell>
                    <Typography variant="body2">{user.userName || "N/A"}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{user.email || "N/A"}</Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={user.roles.join(", ") || "N/A"}
                      size="small"
                      color={user.roles.includes("Admin") ? "primary" : "default"}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatUserProfiles(user.userProfiles)}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{formatDate(user.createdDate)}</Typography>
                  </TableCell>
                  <TableCell align="center">
                    <Box sx={{ display: "flex", justifyContent: "center", gap: 1 }}>
                      <Tooltip title="Copy ID">
                        <IconButton
                          size="small"
                          color="inherit"
                          onClick={() => handleCopyId(user.id)}
                        >
                          <ContentCopy fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </TableCell>
                </TableRow>
              ))}

              {(!data?.items || data.items.length === 0) && (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ py: 3 }}>
                    <Typography variant="body1" color="textSecondary">
                      {search ? `No users found for "${search}"` : "No users found"}
                    </Typography>
                    {search && (
                      <Button
                        startIcon={<ClearIcon />}
                        onClick={handleClearSearch}
                        sx={{ mt: 2 }}
                      >
                        Clear Search
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Pagination */}
        {data?.pagination && data.items.length > 0 && (
          <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mt: 3 }}>
            <Typography variant="body2" color="textSecondary">
              Showing {calculateStartIndex(data.pagination)} - {calculateEndIndex(data.pagination)} of{" "}
              {data.pagination.totalCount} users
            </Typography>
            <Pagination
              count={data.pagination.totalPages}
              page={data.pagination.currentPage}
              onChange={handlePageChange}
              color="primary"
              shape="rounded"
            />
          </Box>
        )}
      </Paper>

      {/* User Profile Details Dialog */}
      <Dialog open={isProfileDialogOpen} onClose={handleCloseProfileDialog} fullWidth maxWidth="sm">
        <DialogTitle>User Profile Details</DialogTitle>
        <DialogContent dividers>
          {selectedProfile && (
            <Grid container spacing={2}>
              <Grid size={{ xs: 12 }}>
                <TextField
                  label="Profile ID"
                  fullWidth
                  value={selectedProfile.id || "N/A"}
                  InputProps={{ readOnly: true }}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  label="First Name"
                  fullWidth
                  value={selectedProfile.firstName || "N/A"}
                  InputProps={{ readOnly: true }}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  label="Last Name"
                  fullWidth
                  value={selectedProfile.lastName || "N/A"}
                  InputProps={{ readOnly: true }}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  label="Phone Number"
                  fullWidth
                  value={selectedProfile.phoneNumber || "N/A"}
                  InputProps={{ readOnly: true }}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  label="Address"
                  fullWidth
                  value={selectedProfile.address || "N/A"}
                  InputProps={{ readOnly: true }}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  label="Created Date"
                  fullWidth
                  value={formatDate(selectedProfile.createdDate)}
                  InputProps={{ readOnly: true }}
                  margin="normal"
                />
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseProfileDialog}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}