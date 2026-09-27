export interface IPagedResult<T> {
    items: T[];
    pagination: {
        totalCount: number;
        totalPages: number;
        itemsPerPage: number;
        currentPage: number;
    };
}
