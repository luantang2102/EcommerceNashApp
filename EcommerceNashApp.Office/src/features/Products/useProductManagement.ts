import { useState, useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../app/store/store";
import {
  setParams,
  setPageNumber,
  setCreateFormOpen,
  setSelectedProductId,
  setDeleteDialogOpen,
} from "./productsSlice";
import {
  useFetchProductByIdQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
} from "../../app/api/productApi";
import { Product } from "../../app/models/product";

export default function useProductManagement() {
  const dispatch = useAppDispatch();
  const { selectedProductId } = useAppSelector((state) => state.product);
  const { data: selectedProduct, isLoading: isLoadingProduct } = useFetchProductByIdQuery(selectedProductId || "", {
    skip: !selectedProductId,
  });

  const [createProduct, { isLoading: isCreating }] = useCreateProductMutation();
  const [updateProduct, { isLoading: isUpdating }] = useUpdateProductMutation();
  const [deleteProduct, { isLoading: isDeleting }] = useDeleteProductMutation();

  const [formData, setFormData] = useState<Partial<Product>>({
    name: "",
    description: "",
    price: 0,
    inStock: true,
    stockQuantity: 0,
    categories: [],
  });
  const [selectedFiles, setSelectedFiles] = useState<FileList | null>(null);
  const [deletedImageIds, setDeletedImageIds] = useState<string[]>([]);
  const [notification, setNotification] = useState({
    open: false,
    message: "",
    severity: "success" as "success" | "error" | "info" | "warning",
  });

  useEffect(() => {
    if (selectedProductId && selectedProduct) {
      setFormData({
        name: selectedProduct.name,
        description: selectedProduct.description,
        price: selectedProduct.price,
        inStock: selectedProduct.inStock,
        stockQuantity: selectedProduct.inStock ? selectedProduct.stockQuantity : 0, // Ensure 0 if not in stock
        categories: selectedProduct.categories,
      });
      setDeletedImageIds([]);
    } else {
      setFormData({
        name: "",
        description: "",
        price: 0,
        inStock: true,
        stockQuantity: 0,
        categories: [],
      });
      setSelectedFiles(null);
      setDeletedImageIds([]);
    }
  }, [selectedProductId, selectedProduct]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleNumericInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    const parsedValue = value === "" ? 0 : parseFloat(value);
    setFormData((prev) => ({ ...prev, [name]: isNaN(parsedValue) ? 0 : parsedValue }));
  };

  const handleSwitchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: checked,
      ...(name === "inStock" && !checked ? { stockQuantity: 0 } : {}), // Set stockQuantity to 0 if inStock is false
    }));
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setSelectedFiles(e.target.files);
    }
  };

  const handleDeleteExistingImage = (imageId: string) => {
    setDeletedImageIds((prev) => [...prev, imageId]);
  };

  const handleSaveProduct = async () => {
    try {
      const productFormData = new FormData();
      if (formData.name) productFormData.append("name", formData.name);
      if (formData.description) productFormData.append("description", formData.description);
      if (formData.price !== undefined) productFormData.append("price", formData.price.toString());
      if (formData.inStock !== undefined) productFormData.append("inStock", formData.inStock.toString());
      
      // Ensure stockQuantity is 0 if inStock is false, otherwise use the form value
      const stockQuantity = formData.inStock ? (formData.stockQuantity ?? 0) : 0;
      productFormData.append("stockQuantity", stockQuantity.toString());

      if (formData.categories && formData.categories.length > 0) {
        formData.categories.forEach((category) => {
          productFormData.append("categoryIds", category.id);
        });
      }
      if (selectedProductId && selectedProduct?.productImages) {
        const retainedImages = selectedProduct.productImages.filter(
          (image) => !deletedImageIds.includes(image.id)
        );
        retainedImages.forEach((image, index) => {
          productFormData.append(`images[${index}].id`, image.id);
          productFormData.append(`images[${index}].isMain`, image.isMain.toString());
        });
      }
      if (selectedFiles) {
        for (let i = 0; i < selectedFiles.length; i++) {
          productFormData.append("formImages", selectedFiles[i]);
        }
      }

      if (selectedProductId) {
        await updateProduct({ id: selectedProductId, data: productFormData }).unwrap();
        setNotification({ open: true, message: "Product updated successfully", severity: "success" });
      } else {
        await createProduct(productFormData).unwrap();
        setNotification({ open: true, message: "Product created successfully", severity: "success" });
      }

      handleCloseForm();
    } catch (err) {
      console.error("Failed to save product:", err);
      setNotification({ open: true, message: "Failed to save product", severity: "error" });
    }
  };

  const handleConfirmDelete = async () => {
    if (selectedProductId) {
      try {
        await deleteProduct(selectedProductId).unwrap();
        dispatch(setSelectedProductId(null));
        setNotification({ open: true, message: "Product deleted successfully", severity: "success" });
        handleCloseDeleteDialog();
      } catch (err) {
        console.error("Failed to delete product:", err);
        setNotification({ open: true, message: "Failed to delete product", severity: "error" });
      }
    }
  };

  const handleSearchChange = (value: string) => {
    dispatch(setParams({ search: value }));
  };

  const handlePageChange = (page: number) => {
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

  return {
    formData,
    selectedFiles,
    setSelectedFiles,
    deletedImageIds,
    isLoadingProduct,
    isCreating,
    isUpdating,
    isDeleting,
    notification,
    handleInputChange,
    handleNumericInputChange,
    handleSwitchChange,
    handleFileChange,
    handleDeleteExistingImage,
    handleSaveProduct,
    handleConfirmDelete,
    handleSearchChange,
    handlePageChange,
    handleCreateClick,
    handleEditClick,
    handleDeleteClick,
    handleCloseForm,
    handleCloseDeleteDialog,
    handleCloseNotification,
    selectedProduct,
  };
}