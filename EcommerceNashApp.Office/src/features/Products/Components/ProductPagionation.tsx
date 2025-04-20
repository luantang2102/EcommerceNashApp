import { Box, Pagination, Typography } from "@mui/material";
import { PaginationParams } from "../../../app/models/params/pagination";
import useProductManagement from "../useProductManagement";

interface ProductPaginationProps {
  pagination: PaginationParams;
}

export default function ProductPagination({ pagination }: ProductPaginationProps) {
  const { handlePageChange } = useProductManagement();

  const calculateStartIndex = () => {
    return (pagination.currentPage - 1) * pagination.pageSize + 1;
  };

  const calculateEndIndex = () => {
    const endIndex = pagination.currentPage * pagination.pageSize;
    return endIndex > pagination.totalCount ? pagination.totalCount : endIndex;
  };

  return (
    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mt: 3 }}>
      <Typography variant="body2" color="textSecondary">
        Showing {calculateStartIndex()} - {calculateEndIndex()} of {pagination.totalCount} products
      </Typography>
      <Pagination
        count={pagination.totalPages}
        page={pagination.currentPage}
        onChange={(_: React.ChangeEvent<unknown>, page: number) => handlePageChange(page)}
        color="primary"
        shape="rounded"
      />
    </Box>
  );
}