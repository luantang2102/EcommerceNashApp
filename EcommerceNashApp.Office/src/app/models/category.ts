export interface Category {
    id: string;
    name: string;
    description: string;
    level: number;
    isActive: boolean;
    createdDate: string;
    updatedDate: string;
    parentCategoryId?: string | null;
    parentCategory: Category | null;
  }