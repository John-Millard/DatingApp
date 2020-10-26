import { NumberFormatStyle } from '@angular/common';

export interface Pagination {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

export class PaginatedResults<T> {
    results: T;
    pagination: Pagination;
}