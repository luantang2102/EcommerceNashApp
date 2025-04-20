export interface ExistingProductImageRequest {
  id: string;
  isMain: boolean;
}

export interface ProductRequest {
  name: string;
  description: string;
  price: number;
  inStock: boolean;
  stockQuantity: number;
  images: ExistingProductImageRequest[];
  formImages?: File[];
  categoryIds: string[];
}