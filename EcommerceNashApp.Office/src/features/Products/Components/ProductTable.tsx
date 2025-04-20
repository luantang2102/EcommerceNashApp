import {
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    Box,
    Typography,
    Chip,
    Rating,
    Tooltip,
    IconButton,
    Divider,
  } from "@mui/material";
  import { Edit as EditIcon, Delete as DeleteIcon, Inventory as InventoryIcon } from "@mui/icons-material";
  import { format } from "date-fns";
  import { Product } from "../../../app/models/product";
import useProductManagement from "../useProductManagement";
  
  interface ProductTableProps {
    products: Product[];
    refetch: () => void;
  }
  
  export default function ProductTable({ products }: ProductTableProps) {
    const { handleEditClick, handleDeleteClick } = useProductManagement();
  
    const formatDate = (dateString: string | null) => {
      if (!dateString) return "N/A";
      return format(new Date(dateString), "MMM dd, yyyy HH:mm");
    };
  
    return (
      <>
        <Divider sx={{ mb: 2 }} />
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
              {products.map((product) => (
                <TableRow
                  key={product.id}
                  sx={{
                    "&:hover": { backgroundColor: "#f9f9f9" },
                    transition: "background-color 0.2s",
                  }}
                >
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
                            borderRadius: "4px",
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
                          whiteSpace: "nowrap",
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
                    <Typography variant="body2">{product.stockQuantity}</Typography>
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: "flex", alignItems: "center" }}>
                      <Rating value={product.averageRating} precision={0.5} size="small" readOnly />
                      <Typography variant="body2" sx={{ ml: 1 }}>
                        ({product.averageRating.toFixed(1)})
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                      {product.categories.map((category) => (
                        <Chip key={category.id} label={category.name} size="small" sx={{ fontSize: "0.7rem" }} />
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
                    <Box sx={{ display: "flex", justifyContent: "center" }}>
                      <Tooltip title="Edit">
                        <IconButton size="small" color="primary" onClick={() => handleEditClick(product.id)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" color="error" onClick={() => handleDeleteClick(product.id)}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </TableCell>
                </TableRow>
              ))}
              {products.length === 0 && (
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
      </>
    );
  }