export interface CategoryParams {
    pageNumber: number;
    pageSize: number;
    search?: string;
    isActive?: boolean;
    level?: number;
    parentCategoryId?: string;
  }