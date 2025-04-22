export interface CategoryParams {
    pageNumber: number;
    pageSize: number;
    searchTerm?: string;
    isActive?: boolean;
    level?: number;
    parentCategoryId?: string;
  }