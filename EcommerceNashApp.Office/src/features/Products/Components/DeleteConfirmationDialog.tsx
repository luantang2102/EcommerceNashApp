import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    Typography,
    CircularProgress,
  } from "@mui/material";
  import useProductManagement from "../useProductManagement";
  
  interface DeleteConfirmationDialogProps {
    open: boolean;
  }
  
  export default function DeleteConfirmationDialog({ open }: DeleteConfirmationDialogProps) {
    const { isDeleting, handleConfirmDelete, handleCloseDeleteDialog } = useProductManagement();
  
    return (
      <Dialog open={open} onClose={handleCloseDeleteDialog}>
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
    );
  }