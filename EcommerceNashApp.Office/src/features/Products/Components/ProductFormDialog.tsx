import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    IconButton,
    Grid,
    TextField,
    FormControlLabel,
    Switch,
    Box,
    Typography,
    Chip,
    InputAdornment,
    CircularProgress,
  } from "@mui/material";
  import { Close as CloseIcon, Save as SaveIcon, Upload as UploadIcon, Delete as DeleteIcon } from "@mui/icons-material";
  import useProductManagement from "../useProductManagement";
  
  interface ProductFormDialogProps {
    open: boolean;
  }
  
  export default function ProductFormDialog({ open }: ProductFormDialogProps) {
    const {
      formData,
      selectedFiles,
      setSelectedFiles,
      deletedImageIds,
      isLoadingProduct,
      isCreating,
      isUpdating,
      selectedProduct,
      handleInputChange,
      handleNumericInputChange,
      handleSwitchChange,
      handleFileChange,
      handleDeleteExistingImage,
      handleSaveProduct,
      handleCloseForm,
    } = useProductManagement();
  
    return (
      <Dialog open={open} onClose={handleCloseForm} fullWidth maxWidth="md">
        <DialogTitle>
          {selectedProduct ? "Edit Product" : "Create New Product"}
          <IconButton
            aria-label="close"
            onClick={handleCloseForm}
            sx={{ position: "absolute", right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>
          {isLoadingProduct ? (
            <Box sx={{ display: "flex", justifyContent: "center", p: 3 }}>
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
                  value={formData.name || ""}
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
                  value={formData.description || ""}
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
                      {selectedFiles.length} file{selectedFiles.length !== 1 ? "s" : ""} selected
                    </Typography>
                    <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, mt: 1 }}>
                      {Array.from(selectedFiles).map((file, index) => (
                        <Chip
                          key={index}
                          label={file.name}
                          size="small"
                          onDelete={() => {
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
                {selectedProduct?.productImages && selectedProduct.productImages.length > 0 && (
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
            ) : selectedProduct ? "Update" : "Create"}
          </Button>
        </DialogActions>
      </Dialog>
    );
  }