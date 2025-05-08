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
  IconButton,
  InputAdornment,
  Tooltip,
  Card,
  CardContent,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Switch,
  FormControlLabel,
  Grid,
  CircularProgress,
  Alert,
  Snackbar,
  Checkbox,
} from "@mui/material";
import {
  Search as SearchIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  AddCircle as AddCircleIcon,
  Refresh as RefreshIcon,
  Close as CloseIcon,
  Save as SaveIcon,
  ContentCopy as ContentCopyIcon,
  Clear as ClearIcon,
} from "@mui/icons-material";
import {
  setParams,
  setPageNumber,
  setCreateFormOpen,
  setSelectedCategoryId,
  setDeleteDialogOpen,
} from "./categoriesSlice";
import { useAppDispatch, useAppSelector } from "../../app/store/store";
import {
  useFetchCategoriesQuery,
  useFetchCategoryByIdQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation,
} from "../../app/api/categoryApi";
import { format } from "date-fns";
import { Category } from "../../app/models/category";
import { PaginationParams } from "../../app/models/params/pagination";
import { debounce } from "lodash";

export default function CategoryList() {
  const dispatch = useAppDispatch();
  const { params, selectedCategoryId, isCreateFormOpen, isDeleteDialogOpen } = useAppSelector(
    (state) => state.category
  );
  const { data, isLoading, error, refetch, isFetching } = useFetchCategoriesQuery(params);
  const [search, setSearch] = useState(params.searchTerm || "");
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<string[]>([]);

  const { data: selectedCategory, isLoading: isLoadingCategory } = useFetchCategoryByIdQuery(
    selectedCategoryId || "",
    {
      skip: !selectedCategoryId || isDeleteDialogOpen,
    }
  );

  const [createCategory, { isLoading: isCreating }] = useCreateCategoryMutation();
  const [updateCategory, { isLoading: isUpdating }] = useUpdateCategoryMutation();
  const [deleteCategory, { isLoading: isDeleting }] = useDeleteCategoryMutation();

  // Form state
  const [formData, setFormData] = useState<Partial<Category>>({
    name: "",
    description: "",
    isActive: true,
    parentCategoryId: null,
  });

  // Validation state
  const [errors, setErrors] = useState<{
    name?: string;
    description?: string;
    parentCategoryId?: string;
  }>({});

  // Notification state
  const [notification, setNotification] = useState({
    open: false,
    message: "",
    severity: "success" as "success" | "error" | "info" | "warning",
  });

  // Debounced search handler
  const debouncedSearch = useCallback(
    debounce((value: string) => {
      dispatch(setParams({ searchTerm: value.trim() || undefined }));
      dispatch(setPageNumber(1));
    }, 500),
    [dispatch]
  );

  // Reset form and errors when dialog opens/closes or selected category changes
  useEffect(() => {
    if (isCreateFormOpen && selectedCategoryId && selectedCategory) {
      setFormData({
        name: selectedCategory.name,
        description: selectedCategory.description,
        isActive: selectedCategory.isActive,
        parentCategoryId: selectedCategory.parentCategoryId,
      });
      setErrors({});
    } else if (isCreateFormOpen && !selectedCategoryId) {
      setFormData({
        name: "",
        description: "",
        isActive: true,
        parentCategoryId: null,
      });
      setErrors({});
    }
  }, [isCreateFormOpen, selectedCategoryId, selectedCategory]);

  // Sync search input with params.search
  useEffect(() => {
    setSearch(params.searchTerm || "");
  }, [params.searchTerm]);

  // Form validation
  const validateForm = () => {
    const newErrors: typeof errors = {};

    // Name validation
    if (!formData.name?.trim()) {
      newErrors.name = "Category name is required.";
    }

    // Description validation
    if (!formData.description?.trim()) {
      newErrors.description = "Description is required.";
    }

    // ParentCategoryId validation
    if (formData.parentCategoryId && !/^[a-zA-Z0-9-]+$/.test(formData.parentCategoryId)) {
      newErrors.parentCategoryId = "Parent Category ID must be alphanumeric or include hyphens.";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Form handlers
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    setErrors((prev) => ({ ...prev, [name]: undefined }));
  };

  const handleSwitchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: checked,
    }));
  };

  // Copy ID handler
  const handleCopyId = (id: string) => {
    navigator.clipboard.writeText(id).then(() => {
      setNotification({
        open: true,
        message: "Category ID copied to clipboard",
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

  // Form submission
  const handleSaveCategory = async () => {
    if (!validateForm()) {
      setNotification({
        open: true,
        message: "Please fix the errors in the form before saving.",
        severity: "error",
      });
      return;
    }

    try {
      const categoryFormData = new FormData();

      if (formData.name) categoryFormData.append("name", formData.name);
      if (formData.description) categoryFormData.append("description", formData.description);
      categoryFormData.append("isActive", (formData.isActive ?? true).toString());
      if (formData.parentCategoryId) categoryFormData.append("parentCategoryId", formData.parentCategoryId);

      if (selectedCategoryId) {
        await updateCategory({ id: selectedCategoryId, data: categoryFormData }).unwrap();
        setNotification({
          open: true,
          message: "Category updated successfully",
          severity: "success",
        });
      } else {
        await createCategory(categoryFormData).unwrap();
        setNotification({
          open: true,
          message: "Category created successfully",
          severity: "success",
        });
      }

      handleCloseForm();
    } catch (err) {
      console.error("Failed to save category:", err);
      setNotification({
        open: true,
        message: "Failed to save category",
        severity: "error",
      });
    }
  };

  // Delete handlers
  const handleConfirmDelete = async () => {
    if (selectedCategoryId) {
      try {
        await deleteCategory(selectedCategoryId).unwrap();
        dispatch(setSelectedCategoryId(null));
        setSelectedCategoryIds((prev) => prev.filter((id) => id !== selectedCategoryId));
        setNotification({
          open: true,
          message: "Category deleted successfully",
          severity: "success",
        });
        handleCloseDeleteDialog();
      } catch (err) {
        console.error("Failed to delete category:", err);
        setNotification({
          open: true,
          message: "Failed to delete category",
          severity: "error",
        });
      }
    }
  };

  const handleDeleteAllSelected = async () => {
    try {
      for (const id of selectedCategoryIds) {
        await deleteCategory(id).unwrap();
      }
      setSelectedCategoryIds([]);
      setNotification({
        open: true,
        message: `${selectedCategoryIds.length} category(ies) deleted successfully`,
        severity: "success",
      });
    } catch (err) {
      console.error("Failed to delete categories:", err);
      setNotification({
        open: true,
        message: "Failed to delete some categories",
        severity: "error",
      });
    }
  };

  const handleCheckboxChange = (id: string) => {
    setSelectedCategoryIds((prev) =>
      prev.includes(id) ? prev.filter((cid) => cid !== id) : [...prev, id]
    );
  };

  const handleSelectAllChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      const allCategoryIds = data?.items.map((category) => category.id) || [];
      setSelectedCategoryIds(allCategoryIds);
    } else {
      setSelectedCategoryIds([]);
    }
  };

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

  // Dialog controls
  const handleCreateClick = () => {
    dispatch(setCreateFormOpen(true));
    dispatch(setSelectedCategoryId(null));
  };

  const handleEditClick = (id: string) => {
    dispatch(setSelectedCategoryId(id));
    dispatch(setCreateFormOpen(true));
  };

  const handleDeleteClick = (id: string) => {
    dispatch(setSelectedCategoryId(id));
    dispatch(setDeleteDialogOpen(true));
  };

  const handleCloseForm = () => {
    dispatch(setCreateFormOpen(false));
    dispatch(setSelectedCategoryId(null));
    setFormData({
      name: "",
      description: "",
      isActive: true,
      parentCategoryId: null,
    });
    setErrors({});
  };

  const handleCloseDeleteDialog = () => {
    dispatch(setDeleteDialogOpen(false));
    dispatch(setSelectedCategoryId(null));
  };

  const handleCloseNotification = () => {
    setNotification({ ...notification, open: false });
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

  if (isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", p: 5 }}>
        <CircularProgress />
        <Typography variant="h6" sx={{ ml: 2 }}>
          Loading categories...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Card sx={{ p: 3, m: 2, bgcolor: "#fff4f4" }}>
        <CardContent>
          <Typography variant="h6" color="error">
            Error loading categories
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
        Category Management
      </Typography>

      {/* Main Content */}
      <Paper elevation={2} sx={{ p: 3, mb: 4 }}>
        {/* Search and Add Button */}
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
          <TextField
            label="Search Categories"
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
          <Box sx={{ display: "flex", gap: 1 }}>
            {selectedCategoryIds.length > 0 && (
              <Button
                variant="contained"
                color="error"
                onClick={handleDeleteAllSelected}
                startIcon={<DeleteIcon />}
                sx={{ borderRadius: "8px", textTransform: "none" }}
                disabled={isDeleting}
              >
                Delete Selected ({selectedCategoryIds.length})
              </Button>
            )}
            <Button
              variant="contained"
              color="primary"
              onClick={handleCreateClick}
              startIcon={<AddCircleIcon />}
              sx={{ borderRadius: "8px", textTransform: "none" }}
            >
              Add New Category
            </Button>
          </Box>
        </Box>

        <Divider sx={{ mb: 2 }} />

        {/* Categories Table */}
        <TableContainer component={Paper} elevation={0} sx={{ mb: 2 }}>
          <Table sx={{ minWidth: 650 }}>
            <TableHead>
              <TableRow>
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={
                      (data?.items?.length ?? 0) > 0 &&
                      selectedCategoryIds.length === data?.items.length
                    }
                    indeterminate={
                      selectedCategoryIds.length > 0 &&
                      selectedCategoryIds.length < (data?.items.length ?? 0)
                    }
                    onChange={handleSelectAllChange}
                  />
                </TableCell>
                <TableCell>Name</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>Level</TableCell>
                <TableCell align="center">Status</TableCell>
                <TableCell>Parent Category</TableCell>
                <TableCell>Created Date</TableCell>
                <TableCell>Updated Date</TableCell>
                <TableCell align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.items.map((category) => (
                <TableRow key={category.id}>
                  <TableCell padding="checkbox">
                    <Checkbox
                      checked={selectedCategoryIds.includes(category.id)}
                      onChange={() => handleCheckboxChange(category.id)}
                    />
                  </TableCell>
                  <TableCell>
                    <Typography
                      variant="body1"
                      sx={{
                        maxWidth: "150px",
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                        whiteSpace: "nowrap",
                      }}
                    >
                      {category.name}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Tooltip title={category.description}>
                      <Typography
                        variant="body2"
                        sx={{
                          maxWidth: "200px",
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                          whiteSpace: "nowrap",
                        }}
                      >
                        {category.description}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{category.level}</Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={category.isActive ? "Active" : "Inactive"}
                      size="small"
                      color={category.isActive ? "success" : "error"}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {category.parentCategoryName ? category.parentCategoryName : "None"}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{formatDate(category.createdDate)}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{formatDate(category.updatedDate)}</Typography>
                  </TableCell>
                  <TableCell align="center">
                    <Box sx={{ display: "flex", justifyContent: "center", gap: 1 }}>
                      <Tooltip title="Copy ID">
                        <IconButton
                          size="small"
                          color="inherit"
                          onClick={() => handleCopyId(category.id)}
                        >
                          <ContentCopyIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Edit">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => handleEditClick(category.id)}
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteClick(category.id)}
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </TableCell>
                </TableRow>
              ))}

              {(!data?.items || data.items.length === 0) && (
                <TableRow>
                  <TableCell colSpan={9} align="center" sx={{ py: 3 }}>
                    <Typography variant="body1" color="textSecondary">
                      {search ? `No categories found for "${search}"` : "No categories found"}
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
              {data.pagination.totalCount} categories
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

      {/* Create/Edit Form Dialog */}
      <Dialog open={isCreateFormOpen} onClose={handleCloseForm} fullWidth maxWidth="md">
        <DialogTitle>
          {selectedCategoryId ? "Edit Category" : "Create New Category"}
          <IconButton
            aria-label="close"
            onClick={handleCloseForm}
            sx={{ position: "absolute", right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>
          {isLoadingCategory ? (
            <Box sx={{ display: "flex", justifyContent: "center", p: 3 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Grid container spacing={3}>
              {selectedCategoryId && (
                <Grid size={{ xs: 12 }}>
                  <TextField
                    label="Category ID"
                    fullWidth
                    value={selectedCategoryId}
                    margin="normal"
                    InputProps={{
                      readOnly: true,
                    }}
                  />
                </Grid>
              )}
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  name="name"
                  label="Category Name"
                  fullWidth
                  required
                  value={formData.name || ""}
                  onChange={handleInputChange}
                  margin="normal"
                  error={!!errors.name}
                  helperText={errors.name}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <FormControlLabel
                  control={
                    <Switch
                      name="isActive"
                      checked={formData.isActive || false}
                      onChange={handleSwitchChange}
                      color="primary"
                    />
                  }
                  label="Active"
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  name="description"
                  label="Description"
                  fullWidth
                  required
                  multiline
                  rows={4}
                  value={formData.description || ""}
                  onChange={handleInputChange}
                  margin="normal"
                  error={!!errors.description}
                  helperText={errors.description}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  name="parentCategoryId"
                  label="Parent Category ID (Optional)"
                  fullWidth
                  value={formData.parentCategoryId || ""}
                  onChange={handleInputChange}
                  margin="normal"
                  error={!!errors.parentCategoryId}
                  helperText={errors.parentCategoryId}
                />
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseForm}>Cancel</Button>
          <Button
            onClick={handleSaveCategory}
            variant="contained"
            startIcon={<SaveIcon />}
            disabled={isCreating || isUpdating}
          >
            {(isCreating || isUpdating) ? (
              <CircularProgress size={24} color="inherit" />
            ) : selectedCategoryId ? (
              "Update"
            ) : (
              "Create"
            )}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={isDeleteDialogOpen} onClose={handleCloseDeleteDialog}>
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this category? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDeleteDialog}>Cancel</Button>
          <Button
            onClick={handleConfirmDelete}
            color="error"
            variant="contained"
            disabled={isDeleting}
          >
            {isDeleting ? <CircularProgress size={24} color="inherit" /> : "Delete"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}