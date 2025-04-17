import { Category } from "./category";
import { ProductImage } from "./productImage";

export interface Product {
    id: string;
    name: string;
    description: string;
    price: number;
    inStock: boolean;
    stockQuantity: number;
    averageRating: number;
    productImages: ProductImage[];
    categories: Category[];
    createdDate: string;
    updatedDate: string | null;
}