import { useState, useEffect } from "react";
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
  Snackbar
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
  Upload as UploadIcon
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
  useDeleteProductMutation
} from "../../app/api/productApi";
import { format } from "date-fns";
import { Product } from "../../app/models/product";
import { PaginationParams } from "../../app/models/params/pagination";

export default function ProductList() {
  const dispatch = useAppDispatch();
  const { params, selectedProductId, isCreateFormOpen, isDeleteDialogOpen } = useAppSelector((state) => state.product);
  const { data, isLoading, error, refetch } = useFetchProductsQuery(params);
  const [search, setSearch] = useState(params.search || "");
  
  const { data: selectedProduct, isLoading: isLoadingProduct } = useFetchProductByIdQuery(selectedProductId || '', {
    skip: !selectedProductId || isDeleteDialogOpen,
  });
  
  const [createProduct, { isLoading: isCreating }] = useCreateProductMutation();
  const [updateProduct, { isLoading: isUpdating }] = useUpdateProductMutation();
  const [deleteProduct, { isLoading: isDeleting }] = useDeleteProductMutation();
  
  // Form state
  const [formData, setFormData] = useState<Partial<Product>>({
    name: '',
    description: '',
    price: 0,
    inStock: true,
    stockQuantity: 0,
    categories: [],
  });
  
  // File uploads
  const [selectedFiles, setSelectedFiles] = useState<FileList | null>(null);
  
  // Track deleted image IDs
  const [deletedImageIds, setDeletedImageIds] = useState<string[]>([]);
  
  // Notification state
  const [notification, setNotification] = useState({
    open: false,
    message: '',
    severity: 'success' as 'success' | 'error' | 'info' | 'warning'
  });
  
  // Reset form when dialog opens/closes or selected product changes
  useEffect(() => {
    if (isCreateFormOpen && selectedProductId && selectedProduct) {
      setFormData({
        name: selectedProduct.name,
        description: selectedProduct.description,
        price: selectedProduct.price,
        inStock: selectedProduct.inStock,
        stockQuantity: selectedProduct.stockQuantity,
        categories: selectedProduct.categories
      });
      setDeletedImageIds([]); // Reset deleted images
    } else if (isCreateFormOpen && !selectedProductId) {
      // Reset form for new product
      setFormData({
        name: '',
        description: '',
        price: 0,
        inStock: true,
        stockQuantity: 0,
        categories: [],
      });
      setSelectedFiles(null);
      setDeletedImageIds([]);
    }
  }, [isCreateFormOpen, selectedProductId, selectedProduct]);
  
  // Form handlers
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value
    }));
  };
  
  const handleNumericInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: parseFloat(value)
    }));
  };
  
  const handleSwitchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: checked
    }));
  };
  
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setSelectedFiles(e.target.files);
    }
  };
  
  // Handle deleting an existing image
  const handleDeleteExistingImage = (imageId: string) => {
    setDeletedImageIds((prev) => [...prev, imageId]);
  };
  
  // Form submission
  const handleSaveProduct = async () => {
    try {
      const productFormData = new FormData();
      
      // Add text fields
      if (formData.name) productFormData.append('name', formData.name);
      if (formData.description) productFormData.append('description', formData.description);
      if (formData.price !== undefined) productFormData.append('price', formData.price.toString());
      if (formData.inStock !== undefined) productFormData.append('inStock', formData.inStock.toString());
      if (formData.stockQuantity !== undefined) productFormData.append('stockQuantity', formData.stockQuantity.toString());
      
      // Add category IDs
      if (formData.categories && formData.categories.length > 0) {
        formData.categories.forEach((category) => {
          productFormData.append('categoryIds', category.id);
        });
      }
      
      // Add existing images (only those not deleted)
      if (selectedProductId && selectedProduct?.productImages) {
        const retainedImages = selectedProduct.productImages.filter(
          (image) => !deletedImageIds.includes(image.id)
        );
        retainedImages.forEach((image, index) => {
          productFormData.append(`images[${index}].id`, image.id);
          productFormData.append(`images[${index}].isMain`, image.isMain.toString());
        });
      }
      
      // Add new files
      if (selectedFiles) {
        for (let i = 0; i < selectedFiles.length; i++) {
          productFormData.append('formImages', selectedFiles[i]);
        }
      }
      
      // Log FormData contents
      console.log('FormData being sent to API:');
      const formDataEntries: { [key: string]: unknown } = {};
      for (const [key, value] of productFormData.entries()) {
        if (value instanceof File) {
          formDataEntries[key] = {
            name: value.name,
            size: value.size,
            type: value.type,
          };
        } else {
          formDataEntries[key] = value;
        }
      }
      console.log(JSON.stringify(formDataEntries, null, 2));

      // Create or update
      if (selectedProductId) {
        await updateProduct({ id: selectedProductId, data: productFormData }).unwrap();
        setNotification({
          open: true,
          message: 'Product updated successfully',
          severity: 'success'
        });
      } else {
        await createProduct(productFormData).unwrap();
        setNotification({
          open: true,
          message: 'Product created successfully',
          severity: 'success'
        });
      }
      
      // Close form
      handleCloseForm();
    } catch (err) {
      console.error('Failed to save product:', err);
      setNotification({
        open: true,
        message: 'Failed to save product',
        severity: 'error'
      });
    }
  };
  
  // Delete confirmation
  const handleConfirmDelete = async () => {
    if (selectedProductId) {
      try {
        const idToDelete = selectedProductId;
        
        await deleteProduct(idToDelete).unwrap();
        
        dispatch(setSelectedProductId(null));
        
        setNotification({
          open: true,
          message: 'Product deleted successfully',
          severity: 'success'
        });
        
        handleCloseDeleteDialog();
      } catch (err) {
        console.error('Failed to delete product:', err);
        setNotification({
          open: true,
          message: 'Failed to delete product',
          severity: 'error'
        });
      }
    }
  };
  
  // Search and pagination
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value);
    dispatch(setParams({ search: e.target.value }));
  };

  const handlePageChange = (_: React.ChangeEvent<unknown>, page: number) => {
    dispatch(setPageNumber(page));
  };

  // Dialog controls
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
      name: '',
      description: '',
      price: 0,
      inStock: true,
      stockQuantity: 0,
      categories: [],
    });
    setSelectedFiles(null);
    setDeletedImageIds([]);
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

  // Calculate pagination values
  const calculateStartIndex = (pagination: PaginationParams) => {
    return (pagination.currentPage - 1) * pagination.pageSize + 1;
  };

  const calculateEndIndex = (pagination: PaginationParams) => {
    const endIndex = pagination.currentPage * pagination.pageSize;
    return endIndex > pagination.totalCount ? pagination.totalCount : endIndex;
  };

  if (isLoading) return (
    <Box sx={{ display: "flex", justifyContent: "center", p: 5 }}>
      <CircularProgress />
      <Typography variant="h6" sx={{ ml: 2 }}>Loading products...</Typography>
    </Box>
  );

  if (error) return (
    <Card sx={{ p: 3, m: 2, bgcolor: "#fff4f4" }}>
      <CardContent>
        <Typography variant="h6" color="error">Error loading products</Typography>
        <Typography variant="body2" sx={{ mt: 1 }}>Please try again later or contact support.</Typography>
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

  return (
    <Box sx={{ p: 3 }}>
      {/* Notification */}
      <Snackbar 
        open={notification.open} 
        autoHideDuration={6000} 
        onClose={handleCloseNotification}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <Alert 
          onClose={handleCloseNotification} 
          severity={notification.severity} 
          sx={{ width: '100%' }}
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
            }}
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
                <TableRow key={product.id} sx={{ 
                  '&:hover': { backgroundColor: '#f9f9f9' },
                  transition: 'background-color 0.2s'
                }}>
                  <TableCell>
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
                            borderRadius: "4px"
                          }}
                        >
                          <InventoryIcon color="disabled" fontSize="small" />
                        </Box>
                      )}
                      <Typography variant="body1">{product.name}</Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Tooltip title={product.description}>
                      <Typography 
                        variant="body2" 
                        sx={{ 
                          maxWidth: "200px", 
                          overflow: "hidden", 
                          textOverflow: "ellipsis", 
                          whiteSpace: "nowrap" 
                        }}
                      >
                        {product.description}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      ${product.price.toFixed(2)}
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
                    <Typography variant="body2">
                      {product.stockQuantity}
                    </Typography>
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
                        />
                      ))}
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatDate(product.createdDate)}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatDate(product.updatedDate)}
                    </Typography>
                  </TableCell>
                  <TableCell align="center">
                    <Box sx={{ display: "flex", justifyContent: "center" }}>
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
                      No products found
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
        
        {/* Pagination */}
        {data?.pagination && (
          <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mt: 3 }}>
            <Typography variant="body2" color="textSecondary">
              Showing {calculateStartIndex(data.pagination)} - {calculateEndIndex(data.pagination)} of {data.pagination.totalCount} products
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
      <Dialog 
        open={isCreateFormOpen} 
        onClose={handleCloseForm}
        fullWidth
        maxWidth="md"
      >
        <DialogTitle>
          {selectedProductId ? 'Edit Product' : 'Create New Product'}
          <IconButton
            aria-label="close"
            onClick={handleCloseForm}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>
          {isLoadingProduct ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  name="name"
                  label="Product Name"
                  fullWidth
                  required
                  value={formData.name || ''}
                  onChange={handleInputChange}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  name="price"
                  label="Price ($)"
                  type="number"
                  fullWidth
                  required
                  value={formData.price || 0}
                  onChange={handleNumericInputChange}
                  margin="normal"
                  InputProps={{
                    startAdornment: <InputAdornment position="start">$</InputAdornment>,
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
                  value={formData.description || ''}
                  onChange={handleInputChange}
                  margin="normal"
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
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
                {selectedFiles && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2">
                      {selectedFiles.length} file{selectedFiles.length !== 1 ? 's' : ''} selected
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 1 }}>
                      {Array.from(selectedFiles).map((file, index) => (
                        <Chip 
                          key={index}
                          label={file.name}
                          size="small"
                          onDelete={() => {
                            // Remove file from selection
                            const dt = new DataTransfer();
                            Array.from(selectedFiles).forEach((f, i) => {
                              if (i !== index) dt.items.add(f);
                            });
                            setSelectedFiles(dt.files.length > 0 ? dt.files : null);
                          }}
                        />
                      ))}
                    </Box>
                  </Box>
                )}
                {selectedProductId && selectedProduct?.productImages && selectedProduct.productImages.length > 0 && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2">Current Images:</Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 1 }}>
                      {selectedProduct.productImages
                        .filter((image) => !deletedImageIds.includes(image.id))
                        .map((image) => (
                          <Box key={image.id} sx={{ position: 'relative' }}>
                            <Box 
                              component="img"
                              src={image.imageUrl}
                              alt="Product"
                              sx={{ 
                                width: 80, 
                                height: 80, 
                                objectFit: 'cover',
                                borderRadius: 1,
                                border: image.isMain ? '2px solid #1976d2' : 'none'
                              }}
                            />
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => handleDeleteExistingImage(image.id)}
                              sx={{ 
                                position: 'absolute', 
                                top: 0, 
                                right: 0,
                                bgcolor: 'rgba(255, 255, 255, 0.7)'
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
            ) : (
              selectedProductId ? 'Update' : 'Create'
            )}
          </Button>
        </DialogActions>
      </Dialog>
      
      {/* Delete Confirmation Dialog */}
      <Dialog 
        open={isDeleteDialogOpen} 
        onClose={handleCloseDeleteDialog}
      >
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
            {isDeleting ? <CircularProgress size={24} color="inherit" /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
