import { Box, TextField, Button, InputAdornment } from "@mui/material";
import { Search as SearchIcon, AddCircle as AddCircleIcon } from "@mui/icons-material";
import { useState } from "react";
import useProductManagement from "../useProductManagement";

export default function ProductSearch() {
  const { handleSearchChange, handleCreateClick } = useProductManagement();
  const [search, setSearch] = useState("");

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value);
    handleSearchChange(e.target.value);
  };

  return (
    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
      <TextField
        label="Search Products"
        value={search}
        onChange={handleInputChange}
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
  );
}