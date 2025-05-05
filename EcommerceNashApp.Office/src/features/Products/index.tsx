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
  Typography,
  Paper,
  Chip,
  Rating,
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
  Select,
  MenuItem,
  InputLabel,
  FormControl,
  Collapse,
  Button,
} from "@mui/material";
import {
  Search as SearchIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  AddCircle as AddCircleIcon,
  Refresh as RefreshIcon,
  Inventory as InventoryIcon,
  Close as CloseIcon,
  Save as SaveIcon,
  Upload as UploadIcon,
  ContentCopy as ContentCopyIcon,
  ExpandMore as ExpandMoreIcon,
  Clear as ClearIcon,
} from "@mui/icons-material";
import {
  setParams,
  setPageNumber,
  setCreateFormOpen,
  setSelectedProductId,
  setDeleteDialogOpen,
} from "./productsSlice";
import { useAppDispatch, useAppSelector } from "../../app/store/store";
import {
  useFetchProductsQuery,
  useFetchProductByIdQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
} from "../../app/api/productApi";
import { useFetchCategoriesTreeQuery } from "../../app/api/categoryApi";
import { format } from "date-fns";
import { Product } from "../../app/models/product";
import { PaginationParams } from "../../app/models/params/pagination";
import { Category } from "../../app/models/category";
import { debounce } from "lodash";

// Recursive Category Menu Component
interface CategoryMenuProps {
  categories: Category[];
  depth: number;
  selectedCategoryIds: { id: string; name: string }[];
  onSelect: (categoryId: string, categoryName: string) => void;
}

const CategoryMenu = ({ categories, depth, selectedCategoryIds, onSelect }: CategoryMenuProps) => {
  const [openMenus, setOpenMenus] = useState<{ [key: string]: boolean }>({});

  const toggleMenu = (categoryId: string) => {
    setOpenMenus((prev) => ({
      ...prev,
      [categoryId]: !prev[categoryId],
    }));
  };

  return (
    <>
      {categories.map((category) => {
        const hasChildren = category.subCategories && category.subCategories.length > 0;
        const isLeaf = !hasChildren;
        const isSelected = selectedCategoryIds.some((item) => item.id === category.id);

        return (
          <Box key={category.id}>
            <MenuItem
              sx={{
                pl: 2 + depth * 2,
                color: isLeaf ? "text.primary" : "text.secondary",
                fontStyle: isLeaf ? "normal" : "italic",
                backgroundColor: isSelected ? "action.selected" : "inherit",
              }}
              selected={isSelected}
              onClick={() => {
                if (isLeaf) {
                  onSelect(category.id, category.name);
                } else {
                  toggleMenu(category.id);
                }
              }}
            >
              <Box sx={{ display: "flex", alignItems: "center", width: "100%" }}>
                <Typography>{category.name}</Typography>
                {hasChildren && (
                  <IconButton
                    size="small"
                    onClick={(e) => {
                      e.stopPropagation();
                      toggleMenu(category.id);
                    }}
                    sx={{ ml: "auto" }}
                  >
                    <ExpandMoreIcon
                      sx={{
                        transform: openMenus[category.id] ? "rotate(180deg)" : "rotate(0deg)",
                        transition: "transform 0.2s",
                      }}
                    />
                  </IconButton>
                )}
              </Box>
            </MenuItem>
            {hasChildren && (
              <Collapse in={openMenus[category.id]} timeout="auto" unmountOnExit>
                <CategoryMenu
                  categories={category.subCategories}
                  depth={depth + 1}
                  selectedCategoryIds={selectedCategoryIds}
                  onSelect={onSelect}
                />
              </Collapse>
            )}
          </Box>
        );
      })}
    </>
  );
};

const formatVND = (price: number) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    minimumFractionDigits: 0, // No decimals for VND
  }).format(price);
};

export default function ProductList() {
  const dispatch = useAppDispatch();
  const { params, selectedProductId, isCreateFormOpen, isDeleteDialogOpen } = useAppSelector(
    (state) => state.product
  );
  const { data, isLoading, error, refetch, isFetching } = useFetchProductsQuery(params);
  const [search, setSearch] = useState(params.searchTerm || "");

  const { data: selectedProduct, isLoading: isLoadingProduct } = useFetchProductByIdQuery(
    selectedProductId || "",
    {
      skip: !selectedProductId || isDeleteDialogOpen,
    }
  );

  const { data: categoriesData, isLoading: isLoadingCategories } = useFetchCategoriesTreeQuery();

  const [createProduct, { isLoading: isCreating }] = useCreateProductMutation();
  const [updateProduct, { isLoading: isUpdating }] = useUpdateProductMutation();
  const [deleteProduct, { isLoading: isDeleting }] = useDeleteProductMutation();

  // Form state
  const [formData, setFormData] = useState<Partial<Product>>({
    name: "",
    description: "",
    price: 0,
    inStock: true,
    stockQuantity: 0,
    categories: [],
  });

  // File uploads
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);

  // Track deleted image IDs
  const [deletedImageIds, setDeletedImageIds] = useState<string[]>([]);

  // Notification state
  const [notification, setNotification] = useState({
    open: false,
    message: "",
    severity: "success" as "success" | "error" | "info" | "warning",
  });

  // Category selection state
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<{ id: string; name: string }[]>([]);

  // Debounced search handler
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const debouncedSearch = useCallback(
    debounce((value: string) => {
      dispatch(setParams({ searchTerm: value.trim() || undefined }));
      dispatch(setPageNumber(1)); // Reset to first page on new search
    }, 500),
    [dispatch]
  );

  // Reset form when dialog opens/closes or selected product changes
  useEffect(() => {
    if (isCreateFormOpen && selectedProductId && selectedProduct) {
      setFormData({
        name: selectedProduct.name,
        description: selectedProduct.description,
        price: selectedProduct.price,
        inStock: selectedProduct.inStock,
        stockQuantity: selectedProduct.stockQuantity,
        categories: selectedProduct.categories,
      });
      setSelectedCategoryIds(
        selectedProduct.categories.map((c) => ({ id: c.id, name: c.name }))
      );
      setSelectedFiles([]);
      setDeletedImageIds([]);
    } else if (isCreateFormOpen && !selectedProductId) {
      setFormData({
        name: "",
        description: "",
        price: 0,
        inStock: true,
        stockQuantity: 0,
        categories: [],
      });
      setSelectedCategoryIds([]);
      setSelectedFiles([]);
      setDeletedImageIds([]);
    }
  }, [isCreateFormOpen, selectedProductId, selectedProduct]);

  // Sync search input with params.search
  useEffect(() => {
    setSearch(params.searchTerm || "");
  }, [params.searchTerm]);

  // Form handlers
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleNumericInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: parseFloat(value),
    }));
  };

  const handleSwitchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: checked,
    }));
  };

  const handleCategorySelect = (categoryId: string, categoryName: string) => {
    setSelectedCategoryIds((prev) => {
      const isSelected = prev.some((item) => item.id === categoryId);
      if (isSelected) {
        return prev.filter((item) => item.id !== categoryId);
      } else {
        return [...prev, { id: categoryId, name: categoryName }];
      }
    });

    setFormData((prev) => {
      const currentCategoryIds = prev.categories?.map((c) => c.id) || [];
      let updatedCategories: Category[] = prev.categories ? [...prev.categories] : [];

      if (currentCategoryIds.includes(categoryId)) {
        updatedCategories = updatedCategories.filter((c) => c.id !== categoryId);
      } else {
        const category = findCategoryById(categoriesData || [], categoryId);
        if (category) {
          updatedCategories.push(category);
        }
      }

      return {
        ...prev,
        categories: updatedCategories,
      };
    });
  };

  const findCategoryById = (categories: Category[], id: string): Category | undefined => {
    for (const category of categories) {
      if (category.id === id) return category;
      if (category.subCategories && category.subCategories.length > 0) {
        const found = findCategoryById(category.subCategories, id);
        if (found) return found;
      }
    }
    return undefined;
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const newFiles = Array.from(e.target.files);
      setSelectedFiles((prev) => [...prev, ...newFiles]);
    }
  };

  const handleDeleteExistingImage = (imageId: string) => {
    setDeletedImageIds((prev) => [...prev, imageId]);
  };

  const handleCopyId = (id: string) => {
    navigator.clipboard.writeText(id).then(() => {
      setNotification({
        open: true,
        message: "Product ID copied to clipboard",
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

  const handleSaveProduct = async () => {
    try {
      const productFormData = new FormData();
      if (formData.name) productFormData.append("Name", formData.name);
      if (formData.description) productFormData.append("Description", formData.description);
      if (formData.price !== undefined) productFormData.append("Price", formData.price.toString());
      productFormData.append("InStock", (formData.inStock ?? true).toString());
      if (formData.stockQuantity !== undefined)
        productFormData.append("StockQuantity", formData.stockQuantity.toString());

      selectedCategoryIds.forEach((category, index) => {
        productFormData.append(`CategoryIds[${index}]`, category.id);
      });

      if (selectedProductId && selectedProduct?.productImages) {
        const retainedImages = selectedProduct.productImages.filter(
          (image) => !deletedImageIds.includes(image.id)
        );
        retainedImages.forEach((image, index) => {
          productFormData.append(`Images[${index}].Id`, image.id);
          productFormData.append(`Images[${index}].IsMain`, image.isMain.toString());
        });
      }

      if (selectedFiles.length > 0) {
        selectedFiles.forEach((file) => {
          productFormData.append("FormImages", file);
        });
      }

      if (selectedProductId) {
        await updateProduct({ id: selectedProductId, data: productFormData }).unwrap();
        setNotification({
          open: true,
          message: "Product updated successfully",
          severity: "success",
        });
      } else {
        await createProduct(productFormData).unwrap();
        setNotification({
          open: true,
          message: "Product created successfully",
          severity: "success",
        });
      }

      handleCloseForm();
    } catch (err) {
      console.error("Failed to save product:", err);
      setNotification({
        open: true,
        message: "Failed to save product",
        severity: "error",
      });
    }
  };

  const handleConfirmDelete = async () => {
    if (selectedProductId) {
      try {
        await deleteProduct(selectedProductId).unwrap();
        dispatch(setSelectedProductId(null));
        setNotification({
          open: true,
          message: "Product deleted successfully",
          severity: "success",
        });
        handleCloseDeleteDialog();
      } catch (err) {
        console.error("Failed to delete product:", err);
        setNotification({
          open: true,
          message: "Failed to delete product",
          severity: "error",
        });
      }
    }
  };

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

  const handleCreateClick = () => {
    dispatch(setCreateFormOpen(true));
    dispatch(setSelectedProductId(null));
  };

  const handleEditClick = (id: string) => {
    dispatch(setSelectedProductId(id));
    dispatch(setCreateFormOpen(true));
  };

  const handleDeleteClick = (id: string) => {
    dispatch(setSelectedProductId(id));
    dispatch(setDeleteDialogOpen(true));
  };

  const handleCloseForm = () => {
    dispatch(setCreateFormOpen(false));
    dispatch(setSelectedProductId(null));
    setFormData({
      name: "",
      description: "",
      price: 0,
      inStock: true,
      stockQuantity: 0,
      categories: [],
    });
    setSelectedFiles([]);
    setDeletedImageIds([]);
    setSelectedCategoryIds([]);
    setAnchorEl(null);
  };

  const handleCloseDeleteDialog = () => {
    dispatch(setDeleteDialogOpen(false));
    dispatch(setSelectedProductId(null));
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
          Loading products...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Card sx={{ p: 3, m: 2, bgcolor: "#fff4f4" }}>
        <CardContent>
          <Typography variant="h6" color="error">
            Error loading products
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
        Product Management
      </Typography>

      {/* Main Content */}
      <Paper elevation={2} sx={{ p: 3, mb: 4 }}>
        {/* Search and Add Button */}
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
          <TextField
            label="Search Products"
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
          <Button
            variant="contained"
            color="primary"
            onClick={handleCreateClick}
            startIcon={<AddCircleIcon />}
            sx={{ borderRadius: "8px", textTransform: "none" }}
          >
            Add New Product
          </Button>
        </Box>

        <Divider sx={{ mb: 2 }} />

        {/* Products Table */}
        <TableContainer component={Paper} elevation={0} sx={{ mb: 2 }}>
          <Table sx={{ minWidth: 650 }}>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>Price</TableCell>
                <TableCell align="center">Stock Status</TableCell>
                <TableCell>Quantity</TableCell>
                <TableCell align="center">Rating</TableCell>
                <TableCell>Categories</TableCell>
                <TableCell>Created Date</TableCell>
                <TableCell>Updated Date</TableCell>
                <TableCell align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.items.map((product) => (
                <TableRow
                  key={product.id}
                >
                  <TableCell>
                    <Tooltip title={product.name}>
                      <Box sx={{ display: "flex", alignItems: "center" }}>
                        {product.productImages && product.productImages.length > 0 ? (
                          <Box
                            component="img"
                            src={product.productImages[0].imageUrl}
                            alt={product.name}
                            sx={{ width: 40, height: 40, mr: 1, objectFit: "cover", borderRadius: "4px" }}
                          />
                        ) : (
                          <Box
                            sx={{
                              width: 40,
                              height: 40,
                              mr: 1,
                              display: "flex",
                              alignItems: "center",
                              justifyContent: "center",
                              bgcolor: "#f0f0f0",
                              borderRadius: "4px",
                            }}
                          >
                            <InventoryIcon color="disabled" fontSize="small" />
                          </Box>
                        )}
                        <Typography
                          variant="body1"
                          sx={{
                            maxWidth: "5vw",
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                            whiteSpace: "nowrap",
                          }}
                        >
                          {product.name}
                        </Typography>
                      </Box>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    <Tooltip title={product.description}>
                      <Typography
                        variant="body2"
                        sx={{
                          maxWidth: "7vw",
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                          whiteSpace: "nowrap",
                        }}
                      >
                        {product.description}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {formatVND(product.price)}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={product.inStock ? "In Stock" : "Out of Stock"}
                      size="small"
                      color={product.inStock ? "success" : "error"}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell align="center">
                    <Typography variant="body2">{product.stockQuantity}</Typography>
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: "flex", alignItems: "center" }}>
                      <Rating
                        value={product.averageRating}
                        precision={0.5}
                        size="small"
                        readOnly
                      />
                      <Typography variant="body2" sx={{ ml: 1 }}>
                        ({product.averageRating.toFixed(1)})
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                      {product.categories.map((category) => (
                        <Chip
                          key={category.id}
                          label={category.name}
                          size="small"
                          sx={{ fontSize: "0.7rem" }}
                          color={category.subCategories.length > 0 ? "error" : "success"}
                        />
                      ))}
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{formatDate(product.createdDate)}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{formatDate(product.updatedDate)}</Typography>
                  </TableCell>
                  <TableCell align="center">
                    <Box sx={{ display: "flex", justifyContent: "center", gap: 1 }}>
                      <Tooltip title="Copy ID">
                        <IconButton
                          size="small"
                          color="inherit"
                          onClick={() => handleCopyId(product.id)}
                        >
                          <ContentCopyIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Edit">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => handleEditClick(product.id)}
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteClick(product.id)}
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
                  <TableCell colSpan={10} align="center" sx={{ py: 3 }}>
                    <Typography variant="body1" color="textSecondary">
                      {search ? `No products found for "${search}"` : "No products found"}
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
              {data.pagination.totalCount} products
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
          {selectedProductId ? "Edit Product" : "Create New Product"}
          <IconButton
            aria-label="close"
            onClick={handleCloseForm}
            sx={{ position: "absolute", right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>
          {isLoadingProduct || isLoadingCategories ? (
            <Box sx={{ display: "flex", justifyContent: "center", p: 3 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Grid container spacing={3}>
              {selectedProductId && (
                <Grid size={{ xs: 12 }}>
                  <TextField
                    label="Product ID"
                    fullWidth
                    value={selectedProductId}
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
                  label="Product Name"
                  fullWidth
                  required
                  value={formData.name || ""}
                  onChange={handleInputChange}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                  name="price"
                  label="Price (VND)" // Updated label to reflect VND
                  type="number"
                  fullWidth
                  required
                  value={formData.price || 0}
                  onChange={handleNumericInputChange}
                  margin="normal"
                  InputProps={{
                    endAdornment: <InputAdornment position="end">₫</InputAdornment>, // Changed to VND symbol
                    inputProps: { step: 1, min: 0 }, // Ensure whole numbers
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  name="description"
                  label="Description"
                  fullWidth
                  multiline
                  rows={4}
                  value={formData.description || ""}
                  onChange={handleInputChange}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <FormControl fullWidth margin="normal">
                  <InputLabel id="categories-label">Categories</InputLabel>
                  <Select
                    labelId="categories-label"
                    open={Boolean(anchorEl)}
                    onOpen={(event) => setAnchorEl(event.currentTarget as HTMLElement)}
                    onClose={() => setAnchorEl(null)}
                    value={selectedCategoryIds.map((item) => item.id)}
                    multiple
                    renderValue={() => (
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                        {selectedCategoryIds.map((category) => (
                          <Chip
                            key={category.id}
                            label={`${category.name}`}
                            size="small"
                            sx={{ bgcolor: "grey.200", color: "text.primary" }}
                          />
                        ))}
                      </Box>
                    )}
                  >
                    <CategoryMenu
                      categories={categoriesData || []}
                      depth={0}
                      selectedCategoryIds={selectedCategoryIds}
                      onSelect={handleCategorySelect}
                    />
                  </Select>
                </FormControl>
              </Grid>
              <Grid size={{ xs: 12, sm: 8 }}>
                <FormControlLabel
                  control={
                    <Switch
                      name="inStock"
                      checked={formData.inStock || false}
                      onChange={handleSwitchChange}
                      color="primary"
                    />
                  }
                  label="In Stock"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  name="stockQuantity"
                  label="Stock Quantity"
                  type="number"
                  fullWidth
                  value={formData.stockQuantity || 0}
                  onChange={handleNumericInputChange}
                  margin="normal"
                  disabled={!formData.inStock}
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <Button
                  variant="outlined"
                  component="label"
                  startIcon={<UploadIcon />}
                  sx={{ mt: 2 }}
                >
                  Upload Images
                  <input
                    type="file"
                    hidden
                    multiple
                    accept="image/*"
                    onChange={handleFileChange}
                  />
                </Button>
                {selectedFiles.length > 0 && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2">
                      {selectedFiles.length} file{selectedFiles.length !== 1 ? "s" : ""} selected
                    </Typography>
                    <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, mt: 1 }}>
                      {selectedFiles.map((file, index) => (
                        <Chip
                          key={index}
                          label={file.name}
                          size="small"
                          onDelete={() => {
                            setSelectedFiles((prev) => prev.filter((_, i) => i !== index));
                          }}
                        />
                      ))}
                    </Box>
                  </Box>
                )}
                {selectedProductId &&
                  selectedProduct?.productImages &&
                  selectedProduct.productImages.length > 0 && (
                    <Box sx={{ mt: 2 }}>
                      <Typography variant="body2">Current Images:</Typography>
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, mt: 1 }}>
                        {selectedProduct.productImages
                          .filter((image) => !deletedImageIds.includes(image.id))
                          .map((image) => (
                            <Box key={image.id} sx={{ position: "relative" }}>
                              <Box
                                component="img"
                                src={image.imageUrl}
                                alt="Product"
                                sx={{
                                  width: 80,
                                  height: 80,
                                  objectFit: "cover",
                                  borderRadius: 1,
                                  border: image.isMain ? "2px solid #1976d2" : "none",
                                }}
                              />
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => handleDeleteExistingImage(image.id)}
                                sx={{
                                  position: "absolute",
                                  top: 0,
                                  right: 0,
                                  bgcolor: "rgba(255, 255, 255, 0.7)",
                                }}
                              >
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </Box>
                          ))}
                      </Box>
                    </Box>
                  )}
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseForm}>Cancel</Button>
          <Button
            onClick={handleSaveProduct}
            variant="contained"
            startIcon={<SaveIcon />}
            disabled={isCreating || isUpdating || !formData.name || formData.price === undefined}
          >
            {(isCreating || isUpdating) ? (
              <CircularProgress size={24} color="inherit" />
            ) : selectedProductId ? (
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
            Are you sure you want to delete this product? This action cannot be undone.
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